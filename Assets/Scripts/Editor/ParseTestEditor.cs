using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParseTest))]
public class ParseTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Run All Tests"))
        { 
            ParseTest parseTest = (ParseTest)this.target;

            for(int iCat = 0; iCat < parseTest.testCategories.Count; ++iCat)
            {
                ParseTest.TestCategory category = parseTest.testCategories[iCat];

                bool nominal = true;
                Debug.Log($"Running tests for category {category.title}");


                for(int iTest = 0; iTest < category.tests.Count; ++iTest)
                {
                    ParseTest.ScriptTest test = category.tests[iTest];

                    LogSession session = new LogSession();
                    session.AppendLine($"Batch test {category.title} - {iCat}/{iTest} - {test.title}");
                    if(!test.Test(session))
                    { 
                        nominal = false;
                        break;
                    }
                }
                if(!nominal)
                    break;
            }
        }
    }
}
