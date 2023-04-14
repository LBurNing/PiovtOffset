#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TestSaveSprite
{
    static void SaveSprite()
    {
        foreach (Object obj in Selection.objects)
        {
            string selectionPath = AssetDatabase.GetAssetPath(obj);
            Object[] texture2D = AssetDatabase.LoadAllAssetsAtPath(selectionPath);

            // ���������ļ���
            string outPath = Application.dataPath + "/../outSprite/";
            System.IO.Directory.CreateDirectory(outPath);

            foreach (Object objSprite in texture2D)
            {
                Sprite sprite = objSprite as Sprite;
                if (sprite == null)
                    continue;

                // ��������������
                Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, sprite.texture.format, false);
                tex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.xMin, (int)sprite.rect.yMin,
                    (int)sprite.rect.width, (int)sprite.rect.height));

                tex.Apply();

                // д���PNG�ļ�
                System.IO.File.WriteAllBytes(outPath + "/" + sprite.name + ".png", tex.EncodeToPNG());
            }
        }

    }
}
#endif