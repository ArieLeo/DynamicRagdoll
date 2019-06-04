﻿using UnityEngine;
using UnityEditor;

namespace DynamicRagdoll {

    [CustomEditor(typeof(RagdollControllerProfile))]
    public class RagdollControllerProfileEditor : Editor {
        static bool[] showBones = new bool[Ragdoll.bonesCount];
        static void DrawPropertiesBlock(SerializedProperty baseProp, string[] names) {
            EditorGUI.indentLevel++;
            for (int i = 0; i < names.Length; i++) {
                EditorGUILayout.PropertyField(baseProp.FindPropertyRelative(names[i]));   
            }
            EditorGUI.indentLevel--;        
        }
        static void DrawPropertiesBlock(SerializedObject baseProp, string label, string[] names) {
            EditorGUILayout.LabelField("<b>"+label+":</b>", RagdollEditor.labelStyle);
            EditorGUI.indentLevel++;
            for (int i = 0; i < names.Length; i++) {
                EditorGUILayout.PropertyField(baseProp.FindProperty(names[i]));   
            }
            EditorGUI.indentLevel--;        
        }
        public static void DrawProfile (SerializedObject profile) {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("<b>Controller Profile Values:</b>", RagdollEditor.labelStyle);

            RagdollEditor.StartBox();
            
            EditorGUI.indentLevel++;

            SerializedProperty boneProfiles = profile.FindProperty("bones");
            
            RagdollEditor.StartBox();
            
            for (int i = 0; i < boneProfiles.arraySize; i++) {
                if (i == 3 || i == 7) {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                        
                    RagdollEditor.StartBox();
                }
                 
                SerializedProperty boneProfile = boneProfiles.GetArrayElementAtIndex(i);
                SerializedProperty bone = boneProfile.FindPropertyRelative("bone");
                    
                showBones[i] = EditorGUILayout.Foldout(showBones[i], "<b>" + bone.enumDisplayNames[bone.enumValueIndex] + ":</b>", RagdollEditor.foldoutStyle);
                
                if (showBones[i]) {
                    
                    DrawPropertiesBlock(boneProfile, i == 0 ? new string[] { "fallForceDecay" } : new string[] { "fallForceDecay", "fallTorqueDecay" });
                
                    EditorGUILayout.BeginHorizontal();
                    
                    GUILayout.FlexibleSpace();

                    DrawBoneProfileNeighbors(boneProfile.FindPropertyRelative("neighbors"), (HumanBodyBones)bone.enumValueIndex);
                    
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
                        
            EditorGUILayout.Space();

            DrawPropertiesBlock(profile, "Falling", new string[] { "maxTorque", "fallDecaySpeed", "maxGravityAddVelocity", "loseFollowDot" } );

		    DrawPropertiesBlock(profile, "Get Up", new string[] { "ragdollMinTime", "settledSpeed", "orientateDelay", "checkGroundMask", "blendTime" });
            
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();   
            
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();     

            if (EditorGUI.EndChangeCheck()) {
                profile.ApplyModifiedProperties();
                EditorUtility.SetDirty(profile.targetObject);
            }
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            DrawProfile(serializedObject);
        }

        static void DrawBoneProfileNeighbors (SerializedProperty neighborsProp, HumanBodyBones baseBone) {
            

            if (GUILayout.Button(new GUIContent("Neighbors", "Define which bones count as neighbors for other bones (for the bone decay system)"), EditorStyles.miniButton)) {
                int neighborsLength = neighborsProp.arraySize;
                System.Func<HumanBodyBones, bool> containsBone = (b) => {
                    int bi = (int)b;
                    for (int i = 0; i < neighborsLength; i++) {
                        if (neighborsProp.GetArrayElementAtIndex(i).enumValueIndex == bi) {
                            return true;
                        }
                    }
                    return false;
                };
                System.Func<HumanBodyBones, int> indexOf = (b) => {
                    int bi = (int)b;
                    for (int i = 0; i < neighborsLength; i++) {
                        if (neighborsProp.GetArrayElementAtIndex(i).enumValueIndex == bi) {
                            return i;
                        }
                    }
                    return -1;
                };

                System.Action<HumanBodyBones> removeBone = (b) => {
                    neighborsProp.DeleteArrayElementAtIndex(indexOf(b));
                };
            
                System.Action<HumanBodyBones> addBone = (b) => {
                    neighborsProp.InsertArrayElementAtIndex(neighborsLength);
                    neighborsProp.GetArrayElementAtIndex(neighborsLength).enumValueIndex = (int)b;
                };

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < Ragdoll.bonesCount; i++) {
                    HumanBodyBones hb = Ragdoll.humanBones[i];
                    if (hb == baseBone) {
                        continue;
                    }


                    menu.AddItem(new GUIContent(hb.ToString()), containsBone(hb), 
                        (b) => {
                        HumanBodyBones hb2 = (HumanBodyBones)b;
                        if (containsBone(hb2)) {
                            removeBone(hb2);
                        }
                        else {
                            addBone(hb2);
                        }

                    }, hb);
                }
                
                // display the menu
                menu.ShowAsContext();
            }
        }
    }
}