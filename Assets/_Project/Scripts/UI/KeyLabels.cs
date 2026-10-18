using System.Collections.Generic;

namespace YesChef.UI
{
    /// <summary>
    /// Turns a binding's display label into key caps for the controls table.
    /// <para>
    /// Single-key composites such as "W/A/S/D" become one cap per key. Composites made of
    /// words ("Up/Left/Down/Right") would sprawl into a dozen caps, so they collapse to a
    /// single readable cap - the arrow keys get a friendly name, anything else is joined.
    /// </para>
    /// </summary>
    public static class KeyLabels
    {
        private const string ArrowKeysLabel = "Arrow Keys";

        public static IEnumerable<string> SplitIntoCaps(string label)
        {
            string[] parts = label.Split('/');

            if (parts.Length == 1)
            {
                yield return label.Trim();
                yield break;
            }

            bool allSingleKeys = true;
            bool allArrows = true;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                allSingleKeys &= part.Length == 1;
                allArrows &= IsArrowKey(part);
            }

            if (allSingleKeys)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    yield return parts[i].Trim();
                }

                yield break;
            }

            yield return allArrows ? ArrowKeysLabel : string.Join(" / ", parts).Trim();
        }

        /// <summary>Input System 1.18 names the arrows "Up"; 1.20 names them "Up Arrow".</summary>
        private static bool IsArrowKey(string part)
        {
            const string suffix = " Arrow";
            if (part.EndsWith(suffix))
            {
                part = part.Substring(0, part.Length - suffix.Length);
            }

            return part is "Up" or "Down" or "Left" or "Right";
        }
    }
}
