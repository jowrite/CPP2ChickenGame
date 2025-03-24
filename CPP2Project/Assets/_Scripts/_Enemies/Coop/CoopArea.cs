using System.Collections;
using UnityEngine;

public class CoopArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float captureHeightOffset = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
        { 
            Collectible collectible = other.GetComponent<Collectible>();
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();

            if (collectible == null || enemyAI == null) return;
            if (enemyAI.CurrentState == EnemyAI.AIState.Captured) return;
            
            StartCoroutine(ProcessCapture(other.gameObject, enemyAI, collectible));
          
        }
    }

    private IEnumerator ProcessCapture(GameObject chicken, EnemyAI enemyAI, Collectible collectible)
    {
        //Disable physics and movement
        if (chicken.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;  
        }

        enemyAI.ChangeState(EnemyAI.AIState.Captured);

        // Smooth transition to coop position
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = chicken.transform.position;
        Vector3 endPos = transform.position + Vector3.up * captureHeightOffset;

        while (elapsed < duration)
        {
            chicken.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Finalize capture
        ChickenManager.instance.CaptureChicken(collectible);
        chicken.SetActive(false); // Or destroy after effects
    }
}
