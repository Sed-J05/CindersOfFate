using UnityEditor;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// QA fix (Prompt 18): every Shatranj menu item that creates or saves scenes, prefabs or assets is greyed out while
    /// the Editor is in (or entering) Play mode — running Build All Scenes during play threw
    /// "InvalidOperationException: This cannot be used during play mode". Read-only tools (Validate Project, Balance
    /// Report) stay available. Unity pairs these validate functions with the items by menu path.
    /// </summary>
    public static class PlayModeMenuGuards
    {
        public static bool NotPlaying => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem("Shatranj/Setup Project", true)] private static bool V00() => NotPlaying;
        [MenuItem("Shatranj/Generate Config Assets", true)] private static bool V01() => NotPlaying;
        [MenuItem("Shatranj/Build Sandbox Scene", true)] private static bool V02() => NotPlaying;
        [MenuItem("Shatranj/Generate Animation Manifest", true)] private static bool V03() => NotPlaying;
        [MenuItem("Shatranj/Generate Placeholder Art", true)] private static bool V04() => NotPlaying;
        [MenuItem("Shatranj/Import & Rebuild Animations", true)] private static bool V05() => NotPlaying;
        [MenuItem("Shatranj/Build UI Prefabs", true)] private static bool V06() => NotPlaying;
        [MenuItem("Shatranj/Build Prefabs", true)] private static bool V07() => NotPlaying;
        [MenuItem("Shatranj/Build Levels", true)] private static bool V08() => NotPlaying;
        [MenuItem("Shatranj/Build LevelTest Scene", true)] private static bool V09() => NotPlaying;
        [MenuItem("Shatranj/Build Cutscene Catalog", true)] private static bool V10() => NotPlaying;
        [MenuItem("Shatranj/Build All Scenes", true)] private static bool V11() => NotPlaying;
        [MenuItem("Shatranj/Build Main Menu Scene", true)] private static bool V12() => NotPlaying;
        [MenuItem("Shatranj/Build Cutscene Scene", true)] private static bool V13() => NotPlaying;
        [MenuItem("Shatranj/Build Windows Player", true)] private static bool V14() => NotPlaying;
        [MenuItem("Shatranj/Build Windows Player (Development)", true)] private static bool V15() => NotPlaying;
    }
}
