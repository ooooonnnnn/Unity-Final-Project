using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Camera
{
    public class MultiCharacterController : MonoBehaviour
    {
        private const string GroundLayerName = "Ground";
        [SerializeField] private List<CharacterComponents> characters;
        [SerializeField] private CharacterComponents spectatorPrefab;
        [SerializeField] private CharacterComponents playerPrefab;

        private Dictionary<int, bool> isSpectator = new();
        private CharacterComponents currentCharacter;
        public UnityEvent<CharacterComponents> OnCharacterChange;


        public CharacterComponents CurrentCharacter => currentCharacter;

        private void Awake()
        {
            ChangeCharacter(1);

            for (int i = 0; i < characters.Count; i++) isSpectator.Add(i, false);
        }
    
        private void OnEnable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnWave.AddListener(HandleWave);
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnWave.RemoveListener(HandleWave);
        }
    
        private void HandleWave()
        {
            if (!currentCharacter)
                return;

            var animator = currentCharacter.GetComponentInChildren<AgentAnimator>();

            if (animator)
            {
                animator.PlayWave();
            }
        }

        public void ChangeCharacter(int agentNumber)
        {
            int index = agentNumber - 1;
            if (index >= characters.Count) return;
            currentCharacter = characters[index];
            OnCharacterChange.Invoke(characters[index]);

            print("controlling " + currentCharacter.gameObject.name);
        }

        public void MoveCharacter()
        {
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Mouse.current.position.value);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);
            if (!Physics.Raycast(ray, out RaycastHit colliderHit, 10000, LayerMask.GetMask(GroundLayerName)))
            {
                return;
            }

            if (!currentCharacter.navMeshAgent.enabled)
                return;
            currentCharacter.navMeshAgent.SetDestination(colliderHit.point);
        }

        public void KillAndSpectate()
        {
            int charIndex = characters.IndexOf(currentCharacter);
            if (isSpectator.ContainsKey(charIndex))
                if (isSpectator[charIndex])
                    return;

            isSpectator[charIndex] = true;

            CharacterComponents spectator =
                Instantiate(spectatorPrefab, currentCharacter.transform.position, currentCharacter.transform.rotation);
            characters[charIndex] = spectator;

            Destroy(currentCharacter.gameObject);
            currentCharacter = spectator;
            OnCharacterChange.Invoke(currentCharacter);
        }

        public void Respawn()
        {
            int charIndex = characters.IndexOf(currentCharacter);
            isSpectator[charIndex] = false;
            Destroy(currentCharacter.gameObject);

            CharacterComponents player =
                Instantiate(playerPrefab, GameManager.Instance.spawnPoint.transform.position, Quaternion.identity);
            characters[charIndex] = player;
            currentCharacter = player;
            OnCharacterChange.Invoke(currentCharacter);
        }
    }
}