using UnityEngine;

public class ClassDbBootstrap : MonoBehaviour
{
    [SerializeField] private ClassTemplateDatabase classDb;

    private void Awake()
    {
        CharacterFactory.ClassDb = classDb;
    }
}
