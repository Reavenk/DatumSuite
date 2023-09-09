using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

//[CustomPropertyDrawer(typeof(ParseTest.TestCategroy))]
//public class CategoryTestDrawer : PropertyDrawer
//{
//    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    //{
//    //    EditorGUILayout.PropertyField(property);
//    //    //base.OnGUI(position, property, label);
//    //    // You can use EditorGUI.PropertyField to draw the properties of your custom class
//    //}
//}

[CustomPropertyDrawer(typeof(ParseTest.ScriptTest))]
public class TestDrawer : PropertyDrawer
{
    //public override VisualElement CreatePropertyGUI(SerializedProperty property)
    //{
    //    //property.find
    //    //VisualElement ret = base.CreatePropertyGUI(property);
    //    //ret.Add( new UnityEditor.pr
    //}

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    { 
        return 180.0f;
    }

    //Regex parseTestExtraction = new Regex(@"testCategories\.Array\.data\[([\d]+)\]\.tests\.Array\.data\[([\d]+)\]");
    public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
    {
        //EditorGUILayout.PropertyField(property);
        //base.OnGUI(position, property, label);
        // You can use EditorGUI.PropertyField to draw the properties of your custom class
        //EditorGUILayout.PropertyField(property);
        var propTitle = property.FindPropertyRelative(nameof(ParseTest.ScriptTest.title));
        var propScript = property.FindPropertyRelative(nameof(ParseTest.ScriptTest.script));
        var propExpression = property.FindPropertyRelative(nameof(ParseTest.ScriptTest.checkExpression));
        var propResult = property.FindPropertyRelative(nameof(ParseTest.ScriptTest.resultExpression));
        var propExpectedType = property.FindPropertyRelative(nameof(ParseTest.ScriptTest.expectedType));
        
        EditorGUI.PropertyField(new Rect(r.x, r.y + 0,  r.width,    20.0f), propTitle);
        EditorGUI.PropertyField(new Rect(r.x, r.y + 20, r.width,    60.0f), propScript);
        EditorGUI.PropertyField(new Rect(r.x, r.y + 80, r.width,    20.0f), propExpression);
        EditorGUI.PropertyField(new Rect(r.x, r.y + 100, r.width,    20.0f), propResult);
        EditorGUI.PropertyField(new Rect(r.x, r.y + 120,r.width,    20.0f), propExpectedType);
        if(GUI.Button(new Rect(r.x, r.y + 140, r.width, 20.0f), "Run"))
        {
            ParseTest.ScriptTest test = (ParseTest.ScriptTest)GetPropertyObject(property);
            using ( LogSession ls = new LogSession())
            {
                test.Test(ls);
            }
        }
    }

    public static readonly System.Text.RegularExpressions.Regex parseData = 
        new System.Text.RegularExpressions.Regex("data\\[([\\d]+)\\]");

    public static object GetPropertyObject(SerializedProperty property)
    {
        string [] paths = property.propertyPath.Split(".");
        Debug.Assert(paths.Length > 0);

        // Sample property path:
        //  "testCategories.Array.data[6].tests.Array.data[0]"
        object obj = property.serializedObject.targetObject;
        for(int i = 0; i < paths.Length; ++i)
        { 
            if(paths[i] == "Array")
            { 
                ++i;
                System.Text.RegularExpressions.Match idxParse = parseData.Match(paths[i]);
                Debug.Assert(idxParse.Success);
                int idx = int.Parse(idxParse.Groups[1].Value);

                if(obj is Array aobj)
                    obj = aobj.GetValue(idx);
                else if(obj is IList lobj)
                    obj = lobj[idx];
                else
                    throw new NotImplementedException();
            }
            else
                obj = obj.GetType().GetField(paths[i]).GetValue(obj);
            
        }
        return obj;
    }
}
