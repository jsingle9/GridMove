using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Classes/Class Template Database")]
public class ClassTemplateDatabase : ScriptableObject
{
    public ClassTemplate[] templates;

    public ClassTemplate Get(string classId)
    {
        if (templates == null) return null;
        for (int i = 0; i < templates.Length; i++)
        {
            var t = templates[i];
            if (t != null && t.classId == classId) return t;
        }
        return null;
    }
}
