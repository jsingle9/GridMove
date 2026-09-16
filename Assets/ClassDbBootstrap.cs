using UnityEngine;

public class ClassDbBootstrap : MonoBehaviour
{
    [SerializeField] private ClassTemplateDatabase classDb;

    private void Awake()
    {

        CharacterFactory.ClassDb = classDb;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"ClassDbBootstrap: ClassDb set = {(classDb != null)}");
    }
}
