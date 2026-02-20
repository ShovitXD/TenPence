using UnityEngine;

public class DepositAltarManager : MonoBehaviour
{
    [Header("Accepted Tags")]
    [SerializeField] private string sCrystalTag = "SCrystal";
    [SerializeField] private string fCrystalTag = "FCrystal";

    [Header("Spawn")]
    [SerializeField] private Transform spawnPointE3;
    [SerializeField] private GameObject rewardPrefab;

    private bool hasSCrystal;
    private bool hasFCrystal;
    private bool rewardSpawned;

    // Called by slots when something enters their trigger
    public void TryDeposit(GameObject obj)
    {
        if (obj == null) return;
        if (rewardSpawned) return;

        bool deposited = false;

        if (!hasSCrystal && obj.CompareTag(sCrystalTag))
        {
            hasSCrystal = true;
            deposited = true;
        }
        else if (!hasFCrystal && obj.CompareTag(fCrystalTag))
        {
            hasFCrystal = true;
            deposited = true;
        }

        // Not a valid deposit (wrong tag or already deposited that type)
        if (!deposited) return;

        // Delete deposited object to avoid clutter
        Destroy(obj);

        // Check completion
        if (hasSCrystal && hasFCrystal)
        {
            SpawnReward();
        }
    }

    private void SpawnReward()
    {
        if (rewardSpawned) return;
        rewardSpawned = true;

        if (rewardPrefab == null || spawnPointE3 == null) return;

        Instantiate(rewardPrefab, spawnPointE3.position, spawnPointE3.rotation);
    }

    // Optional: call this if you want to reset the altar later
    public void ResetDeposits()
    {
        hasSCrystal = false;
        hasFCrystal = false;
        rewardSpawned = false;
    }
}