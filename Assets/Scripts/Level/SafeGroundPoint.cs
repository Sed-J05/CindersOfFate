using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>Docs/05 legend 'S': explicit respawn spot after spikes / pits. Feet position.</summary>
    public class SafeGroundPoint : MonoBehaviour
    {
        [SerializeField] private string roomId = "";

        public string RoomId => roomId;

        public void Configure(string room) => roomId = room;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.9f);
            Gizmos.DrawWireSphere(transform.position + new Vector3(0f, 0.25f, 0f), 0.25f);
        }
    }
}
