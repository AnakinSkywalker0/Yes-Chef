using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using YesChef.Characters;

namespace YesChef.Editor
{
    /// <summary>
    /// One-shot swap of the primitive player body for the rigged Chef model.
    /// Idempotent: running it again on an already swapped player does nothing.
    /// </summary>
    [InitializeOnLoad]
    public static class ChefCharacterSetup
    {
        private const string ModelPath = "Assets/_Project/Art/Models/Chef.fbx";
        private const string ControllerPath = "Assets/_Project/Art/Animation/Chef.controller";
        private const string ScenePath = "Assets/_Project/Scenes/Kitchen.unity";
        private const float ModelScale = 0.85f;
        private const float LocomotionSpeed = 3f;

        static ChefCharacterSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path == ScenePath)
                {
                    Apply();
                }
            };
        }

        [MenuItem("Yes Chef/Apply Chef Character")]
        public static void Apply()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                return;
            }

            var visual = Object.FindFirstObjectByType<PlayerVisual>();
            if (visual == null)
            {
                return;
            }

            var so = new SerializedObject(visual);
            var bodyRoot = so.FindProperty("_bodyRoot").objectReferenceValue as Transform;
            if (bodyRoot == null || so.FindProperty("_animator").objectReferenceValue != null)
            {
                return;
            }

            var controller = BuildController(model);
            if (controller == null)
            {
                return;
            }

            // Remove the capsule, head and hands primitives under the old body root.
            for (int i = bodyRoot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(bodyRoot.GetChild(i).gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, bodyRoot);
            instance.name = "Chef";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * ModelScale;

            if (!instance.TryGetComponent(out Animator animator))
            {
                animator = instance.AddComponent<Animator>();
            }
            if (animator.avatar == null)
            {
                animator.avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            so.FindProperty("_animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(visual.gameObject.scene);
            EditorSceneManager.SaveScene(visual.gameObject.scene);
        }

        [MenuItem("Yes Chef/Diagnose Chef Animator")]
        public static void Diagnose()
        {
            var visual = Object.FindFirstObjectByType<PlayerVisual>();
            if (visual == null)
            {
                Debug.LogWarning("[ChefDiag] No PlayerVisual in the open scene.");
                return;
            }

            var so = new SerializedObject(visual);
            var animator = so.FindProperty("_animator").objectReferenceValue as Animator;
            if (animator == null)
            {
                Debug.LogWarning("[ChefDiag] PlayerVisual._animator is NOT assigned.");
                return;
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine($"[ChefDiag] Animator on '{animator.name}' enabled={animator.enabled} " +
                              $"controller={(animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "NONE")} " +
                              $"avatar={(animator.avatar ? animator.avatar.name + " valid=" + animator.avatar.isValid : "NONE")} " +
                              $"hasBoundPlayables={animator.hasBoundPlayables} speed={animator.speed} " +
                              $"culling={animator.cullingMode} rootMotion={animator.applyRootMotion}");

            var clips = animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.animationClips
                : new AnimationClip[0];
            foreach (var clip in clips)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                int resolved = bindings.Count(b => animator.transform.Find(b.path) != null || string.IsNullOrEmpty(b.path));
                report.AppendLine($"[ChefDiag] clip '{clip.name}' length={clip.length:F2}s loop={clip.isLooping} " +
                                  $"curves={bindings.Length} resolvedToBones={resolved}");
                if (bindings.Length > 0 && resolved == 0)
                {
                    report.AppendLine($"[ChefDiag]   first curve path '{bindings[0].path}' not found under '{animator.name}'");
                }
            }

            if (Application.isPlaying)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                report.AppendLine($"[ChefDiag] play mode: Moving={animator.GetBool("Moving")} " +
                                  $"Carrying={animator.GetBool("Carrying")} normalizedTime={st.normalizedTime:F2} " +
                                  $"isInIdle={st.IsName("Idle")} isInWalk={st.IsName("Walk")} isInCarry={st.IsName("Carry")}");
            }

            Debug.Log(report.ToString());
        }

        private static AnimatorController BuildController(GameObject model)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .ToList();
            AnimationClip Find(string n) => clips.FirstOrDefault(c => c.name.EndsWith(n));
            AnimationClip idle = Find("Idle"), walk = Find("Walk"), carry = Find("Carry");
            if (idle == null || walk == null || carry == null)
            {
                Debug.LogWarning("Chef.fbx is missing an Idle/Walk/Carry clip; character swap skipped.");
                return null;
            }

            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null)
            {
                return existing;
            }

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ControllerPath));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Carrying", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;
            var sIdle = sm.AddState("Idle");
            var sWalk = sm.AddState("Walk");
            var sCarry = sm.AddState("Carry");
            sIdle.motion = idle;
            sWalk.motion = walk;
            sCarry.motion = carry;
            sWalk.speed = LocomotionSpeed;
            sCarry.speed = LocomotionSpeed;
            sm.defaultState = sIdle;

            void Link(AnimatorState from, AnimatorState to, params (string, bool)[] conditions)
            {
                var t = from.AddTransition(to);
                t.hasExitTime = false;
                t.duration = 0.1f;
                foreach (var (param, value) in conditions)
                {
                    t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, param);
                }
            }

            Link(sIdle, sWalk, ("Moving", true), ("Carrying", false));
            Link(sIdle, sCarry, ("Moving", true), ("Carrying", true));
            Link(sWalk, sCarry, ("Carrying", true));
            Link(sCarry, sWalk, ("Carrying", false));
            Link(sWalk, sIdle, ("Moving", false));
            Link(sCarry, sIdle, ("Moving", false));

            AssetDatabase.SaveAssets();
            return controller;
        }
    }
}
