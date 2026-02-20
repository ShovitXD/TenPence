using UnityEngine;

public class IngredientItem : MonoBehaviour
{
    [SerializeField] private string ingredientId;
    public string IngredientId => ingredientId;

    public Rigidbody Rb { get; private set; }
    public Collider Col { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<Collider>();
    }
}