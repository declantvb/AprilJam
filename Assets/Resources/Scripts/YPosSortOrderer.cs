using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class YPosSortOrderer : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    [SerializeField] int Offset;
    [SerializeField] int CurrentSortOrder;
        
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        CurrentSortOrder = (int)(-transform.position.y * 100);
        spriteRenderer.sortingOrder = CurrentSortOrder + Offset;
    }
}
