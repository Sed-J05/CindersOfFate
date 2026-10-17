using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>Docs/05 legend 'P': where a new game / phase starts. Feet position.</summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 1.5f, 0f));
        }
    }
}
