#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DevTools.Editor
{
    /// <summary>
    /// Class used to define the behavior of the TODO & Dev Notes editor window.
    /// </summary>
    public class TODODevNotes : EditorWindow
    {
        #region Variables
        /// <summary>
        /// The editor window used to show the TODO & Dev Notes UI.
        /// </summary>
        static TODODevNotes TODODevNotesWindow;

        /// <summary>
        /// The TODONotesContent asset that contains the entries and settings.
        /// </summary>
        TODONotesContent todoNotesContent;


        /// <summary>
        /// The directory where the todoNotesContent asset is saved.
        /// </summary>
        string todoNotesContentDirectory = "Assets/TODO & Dev Notes/Content/";

        /// <summary>
        /// The path to the todoNotesContent asset.
        /// </summary>
        string todoNotesContentAssetPath = "Assets/TODO & Dev Notes/Content/TODOContentAndSettings.asset";
        /// <summary>
        /// The path to the TODODevNotesWindow icon.
        /// </summary>
        static string todoIconPath = "Assets/TODO & Dev Notes/TODODevNotes-Icon.png";

        /// <summary>
        /// Integer used to define the entryType of each TODO entry.
        /// </summary>
        int currentEntryTypeFilterIndex = 0;

        /// <summary>
        /// Integer used to initialize the entryType of each TODO entry.
        /// </summary>
        int newEntryContentEntryTypeIndex = 0;

        /// <summary>
        /// String used to define the title of each new entry.
        /// </summary>
        string newEntryTitle;

        /// <summary>
        /// String used to define the content of each new entry.
        /// </summary>
        string newEntryContent;

        /// <summary>
        /// Boolean used to define a TODO entry as ready to save to the todoNotesContent asset.
        /// </summary>
        bool saveable;

        /// <summary>
        /// Vector used to define the current scroll position in the TODO & Dev Notes TODODevNotesWindow.
        /// </summary>
        Vector2 scrollPosition = Vector2.zero;

        static GUIContent titleGUIContent = new GUIContent();

        private bool[] entryFoldouts;

        [MenuItem("Window/TODO Dev Notes %t")]
        public static void Init()
        {
            titleGUIContent.text = "TODO";
            titleGUIContent.image = (Texture)AssetDatabase.LoadAssetAtPath(todoIconPath, typeof(Texture));
            // Get existing open TODODevNotes Window or if it doesn't exist, create one.
            TODODevNotesWindow = (TODODevNotes)GetWindow(typeof(TODODevNotes));

            //TODO: Replace with titleContent (GUIContent).
            TODODevNotesWindow.titleContent = titleGUIContent;
            TODODevNotesWindow.autoRepaintOnSceneChange = false;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Draws the TODO & Dev Notes editor Window.
        /// </summary>
        public void OnGUI()
        {
            // Fetch our data if we haven't.
            if (todoNotesContent == null)
            {
                // Fetch our data if the asset can be found.
                todoNotesContent = AssetDatabase.LoadAssetAtPath(todoNotesContentAssetPath, typeof(TODONotesContent)) as TODONotesContent;
                // Create our data asset if it can't be found.
                if (todoNotesContent == null)
                {
                    // Create an instance of the data asset.
                    todoNotesContent = CreateInstance(typeof(TODONotesContent)) as TODONotesContent;
                    // Create the directory to store the data asset.
                    System.IO.Directory.CreateDirectory(Application.dataPath + todoNotesContentDirectory);
                    // Save the data asset to disk.
                    AssetDatabase.CreateAsset(todoNotesContent, todoNotesContentAssetPath);
                    // Mark the GUI as Changed.
                    GUI.changed = true;
                }
            }

            // Display the entry type filter fields.
            string[] entryTypes = new string[todoNotesContent.entryTypes.Count + 1];

            // Define the number of entry types defined.
            string[] entryTypesToSelect = new string[todoNotesContent.entryTypes.Count];

            // Set index 0 of the entry types selector to show all types.
            entryTypes[0] = "All types";

            // Loop through all available entry types.
            for (int i = 0; i < todoNotesContent.entryTypes.Count; i++)
            {
                // Retrieve the name of each entry type and save it as a string.
                entryTypes[i + 1] = todoNotesContent.entryTypes[i].name;
                // Save the name of each entry type to the entryTypesToSelect string array.
                entryTypesToSelect[i] = todoNotesContent.entryTypes[i].name;
            }

            // Show the entry type Filter label and selector in a single row.
            EditorGUILayout.BeginHorizontal();

            // Create the Filter Label.
            EditorGUILayout.LabelField("Filter:", EditorStyles.boldLabel);

            // Show the entry type selector.
            currentEntryTypeFilterIndex = EditorGUILayout.Popup(currentEntryTypeFilterIndex, entryTypes);

            // End the row
            EditorGUILayout.EndHorizontal();
            // Define an integer to keep track of the number of entries and set it to 0.
            int displayCount = 0;
            // Show the list of pending entries.

            // Create a style to use for each entry.
            GUIStyle itemStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);

            // Center our text on each entry.
            itemStyle.alignment = TextAnchor.UpperCenter;

            // Begin the scrollable area that will contain all entries.
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Loop through each item in the list.
            for (int i = 0; i < todoNotesContent.items.Count; i++)
            {
                // Define the current entry instance being looped over.
                ListItem item = todoNotesContent.items[i];

                // Retrieve and define the entry type.
                ListItemEntryType entryType = item.entryType;

                // Code to execute if the filter is set to "All".
                if (currentEntryTypeFilterIndex == 0)
                {
                    //TODO: Move this code to a static container or something so it reuses the textures.

                    // Create a 1x1 single entryColor texture using the entryType entryColor.
                    Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                    // Set the entryColor to the entry type entryColor.
                    colorTexture.SetPixel(0, 0, entryType.entryColor);

                    // Save the changes to the texture.
                    colorTexture.Apply();

                    // Set the item background to the corresponding texture.
                    itemStyle.normal.background = colorTexture;

                    // Set the text entryColor based on the entry type for correct visualization.
                    if (entryType.entryColor == todoNotesContent.todoColor)
                        itemStyle.normal.textColor = Color.black;
                    else
                        itemStyle.normal.textColor = Color.white;

                    // Behaviour to execute if the item is not yet marked as completed.
                    if (item.isFinished == false)
                    {
                        // Add 1 to the display item count.
                        displayCount++;

                        //Show a toggle, a TextArea with the entry contents and an entry type selector in a single row.
                        EditorGUILayout.BeginHorizontal();

                        // Define a toggle that affects the isFinished variable when pressed.
                        if (EditorGUILayout.Toggle(item.isFinished, GUILayout.Width(20)) == true)
                        {
                            // Change the value to true once clicked.
                            item.isFinished = true;
                        }
                        // Define the text area that displays the entry contents.
                        item.entryContent = EditorGUILayout.TextArea(item.entryContent, itemStyle);

                        // Show an entry type selector
                        int entry = EditorGUILayout.Popup(entryType.index, entryTypesToSelect, GUILayout.Width(60));

                        // Handle the change of entry type to a value different than the one currently used
                        if (entry != entryType.index)
                        {
                            // Replace the index used for the new on
                            item.entryType = todoNotesContent.entryTypes[entry];

                            // Set the item to the one currently looped over.
                            // Why? No fucking idea but this was in the original code, I'm just commenting here
                            todoNotesContent.items[i] = item;
                        }

                        // End row
                        EditorGUILayout.EndHorizontal();

                    }
                }

                // Code to execute if the filter is set to whatever besides "All".
                else
                {
                    // Adjust the index by subtracting the 1 this guy added way earlier in the code
                    // My guess is that this prevents some error with "All" at index 0 but your guess is as good as mine
                    int adjustedIndex = currentEntryTypeFilterIndex - 1;

                    // Set the entry type to the one that corresponds to the adjusted index
                    entryType = todoNotesContent.entryTypes[adjustedIndex];

                    // Check if the entry has been correctly set up
                    if (entryType.name == item.entryType.name)
                    {
                        //TODO: Move this code to a static container or something so it reuses the textures.

                        // Create a 1x1 single entryColor texture using the entryType entryColor.
                        Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                        // Set the entryColor to the entry type entryColor.
                        colorTexture.SetPixel(0, 0, entryType.entryColor);

                        // Save the changes to the texture.
                        colorTexture.Apply();

                        // Set the item background to the corresponding texture.
                        itemStyle.normal.background = colorTexture;

                        // Set the text entryColor based on the entry type for correct visualization.
                        if (entryType.entryColor == todoNotesContent.todoColor)
                            itemStyle.normal.textColor = Color.black;
                        else
                            itemStyle.normal.textColor = Color.white;

                        // Behaviour to execute if the item is not yet marked as completed.
                        if (item.isFinished == false)
                        {
                            // Add 1 to the display item count.
                            displayCount++;

                            //Show a toggle, a TextArea with the entry contents and an entry type selector in a single row.
                            EditorGUILayout.BeginHorizontal();

                            // Define a toggle that affects the isFinished variable when pressed.
                            if (EditorGUILayout.Toggle(item.isFinished, GUILayout.Width(20)) == true)
                            {
                                // Change the value to true once clicked.
                                item.isFinished = true;
                            }
                            // Define the text area that displays the entry contents.
                            item.entryContent = EditorGUILayout.TextArea(item.entryContent, itemStyle);

                            // Show an entry type selector
                            int entry = EditorGUILayout.Popup(entryType.index, entryTypesToSelect, GUILayout.Width(60));

                            // Handle the change of entry type to a value different than the one currently used
                            if (entry != entryType.index)
                            {
                                // Replace the index used for the new on
                                item.entryType = todoNotesContent.entryTypes[entry];

                                // Set the item to the one currently looped over.
                                // Why? No fucking idea but this was in the original code, I'm just commenting here
                                todoNotesContent.items[i] = item;
                            }

                            // End row
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            // Code to execute if there are no unfinished entries
            if (displayCount == 0)
            {
                // Print an informative label
                EditorGUILayout.LabelField("No entries", EditorStyles.boldLabel);
            }
            if (currentEntryTypeFilterIndex == 0)
            {
                // Code to execute if there are finished entries
                if (todoNotesContent.finishedItems > 0)
                {
                    // Print an informative label
                    EditorGUILayout.LabelField("Finished entries", EditorStyles.boldLabel);
                }
            }
            // Display Completed Task Section
            for (int i = 0; i < todoNotesContent.items.Count; i++)
            {
                // Define the current entry instance being looped over.
                ListItem item = todoNotesContent.items[i];

                // Retrieve and define the entry type.
                ListItemEntryType entryType = item.entryType;

                if (currentEntryTypeFilterIndex == 0)
                {
                    //TODO: Move this code to a static container or something so it reuses the textures.

                    // Create a 1x1 single entryColor texture using the entryType entryColor.
                    Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                    //TODO: Set the entryColor to the entry type entryColor.

                    // Set the entryColor to the completed entryColor for now.
                    colorTexture.SetPixel(0, 0, entryType.entryFinishedColor);

                    // Save the changes to the texture.
                    colorTexture.Apply();

                    // Set the item background to the corresponding texture.
                    itemStyle.normal.background = colorTexture;

                    // Set the text entryColor based on the entry type for correct visualization.
                    itemStyle.normal.textColor = Color.black;

                    // Code to run in all entries marked as finished.
                    if (item.isFinished == true)
                    {
                        // Code to execute if the item has not been counted as finished.
                        if (!item.countedAsFinished)
                        {
                            // Add 1 to the number of finished items.
                            todoNotesContent.finishedItems++;

                            // Mark the item as counted to avoid adding it once per frame.
                            item.countedAsFinished = true;
                        }

                        // Show a toggle and LabelField with the contents of the finished entry in a single row.
                        EditorGUILayout.BeginHorizontal();

                        // Show completed entry toggle.
                        if (EditorGUILayout.Toggle(item.isFinished, GUILayout.Width(20)) == false)
                        {
                            // Set finished status to false upon click.
                            item.isFinished = false;
                        }
                        // Show the contents of the finished entry but make them non editable this time
                        EditorGUILayout.LabelField(item.entryContent, itemStyle);
                        // Finish row
                        EditorGUILayout.EndHorizontal();

                    }
                    // Code to run in all entries marked as not finished.
                    else if (item.isFinished == false)
                    {
                        // Code to execute if the item has been already counted as finished.
                        if (item.countedAsFinished)
                        {
                            // Subtract 1 from the number of finished items.
                            todoNotesContent.finishedItems--;
                            // Mark the item as not counted to avoid subtracting it once per frame.
                            item.countedAsFinished = false;
                        }
                    }
                }
                else
                {
                    // Adjust the index by subtracting the 1 this guy added way earlier in the code
                    // My guess is that this prevents some error with "All" at index 0 but your guess is as good as mine
                    int adjustedIndex = currentEntryTypeFilterIndex - 1;

                    // Set the entry type to the one that corresponds to the adjusted index
                    entryType = todoNotesContent.entryTypes[adjustedIndex];

                    // Check if the entry has been correctly set up
                    if (entryType.name == item.entryType.name)
                    {
                        //TODO: Move this code to a static container or something so it reuses the textures.

                        // Create a 1x1 single entryColor texture using the entryType entryColor.
                        Texture2D colorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);

                        //TODO: Set the entryColor to the entry type entryColor.

                        // Set the entryColor to the completed entryColor for now.
                        colorTexture.SetPixel(0, 0, entryType.entryFinishedColor);

                        // Save the changes to the texture.
                        colorTexture.Apply();

                        // Set the item background to the corresponding texture.
                        itemStyle.normal.background = colorTexture;

                        // Set the text entryColor based on the entry type for correct visualization.
                        itemStyle.normal.textColor = Color.black;

                        // Code to run in all entries marked as finished.
                        if (item.isFinished == true)
                        {
                            // Code to execute if the item has not been counted as finished.
                            if (!item.countedAsFinished)
                            {
                                // Add 1 to the number of finished items.
                                todoNotesContent.finishedItems++;

                                // Mark the item as counted to avoid adding it once per frame.
                                item.countedAsFinished = true;
                            }

                            // Show a toggle and LabelField with the contents of the finished entry in a single row.
                            EditorGUILayout.BeginHorizontal();

                            // Show completed entry toggle.
                            if (EditorGUILayout.Toggle(item.isFinished, GUILayout.Width(20)) == false)
                            {
                                // Set finished status to false upon click.
                                item.isFinished = false;
                            }
                            // Show the contents of the finished entry but make them non editable this time
                            EditorGUILayout.LabelField(item.entryContent, itemStyle);
                            // Finish row
                            EditorGUILayout.EndHorizontal();

                        }
                        // Code to run in all entries marked as not finished.
                        else if (item.isFinished == false)
                        {
                            // Code to execute if the item has been already counted as finished.
                            if (item.countedAsFinished)
                            {
                                // Subtract 1 from the number of finished items.
                                todoNotesContent.finishedItems--;
                                // Mark the item as not counted to avoid subtracting it once per frame.
                                item.countedAsFinished = false;
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            // Show the new TODO entry creation area in a single row.
            EditorGUILayout.BeginHorizontal();

            // Show a title label.
            EditorGUILayout.LabelField("New entry:", EditorStyles.boldLabel);

            // Show a dropdown selector with the entry types.
            newEntryContentEntryTypeIndex = EditorGUILayout.Popup(newEntryContentEntryTypeIndex, entryTypesToSelect, GUILayout.Width(60));

            // Finish row.
            EditorGUILayout.EndHorizontal();

            // Show a TextArea to input the contents of the new TODO entry.
            newEntryContent = EditorGUILayout.TextArea(newEntryContent, GUILayout.Height(40));

            // Button to create the new TODO entry once some text has been input.
            if ((GUILayout.Button("Create entry") && newEntryContent != ""))
            {
                // Define new TODO entry
                ListItemEntryType newEntryType = todoNotesContent.entryTypes[newEntryContentEntryTypeIndex];
                // Add new entry to the TODO entry list.
                todoNotesContent.AddEntry(newEntryType, newEntryTitle, newEntryContent);
                // Reset the contents of the new entry TextArea once the entry has been added to the list.
                newEntryContent = "";
                // Remove focus control from the GUI?
                // Honestly not sure what this does but my bet is it deselects anything (which would have been the button or TextArea I guess.
                GUI.FocusControl(null);
            }

            // Define the entry or todoNotesContent asset (not sure which tbh) as saveable if the hotcontrol is not 0
            // (?) must be some kind of detection if you're modifying or not the field as it used to save after each gui change
            // but some dude in the asset store provided this fix.
            if (GUIUtility.hotControl != 0) saveable = true;

            // Code to execute once hot control is 0 and it keeps the saveable attribute (see? I assume 0 is no input).
            if (GUIUtility.hotControl == 0 && saveable)
            {
                // Set the attribute to false
                //TODO: Check if this affects the beavior of saving but I assume not, everything seems to run and save fine.
                saveable = false;

                // Set the todoNotesContent asset as dirty.
                EditorUtility.SetDirty(todoNotesContent);

                // Save the todoNotesContent Asset to disk.
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Handles the TODO & Dev Notes behavior when the TODODevNotesWindow is closed.
        /// </summary>
        void OnDestroy()
        {
            // Set the todoNotesContent asset as dirty.
            EditorUtility.SetDirty(todoNotesContent);

            // Save the todoNotesContent Asset to disk.
            AssetDatabase.SaveAssets();
        }

        #endregion

    }

    /// <summary>
    /// A Scriptable Object used to store the settings and entries for the TODO List.
    /// </summary>
    public class TODONotesContent : ScriptableObject
    {
        /// <summary>
        /// The list of item entry types.
        /// </summary>
        public List<ListItemEntryType> entryTypes = new List<ListItemEntryType>();

        /// <summary>
        /// The list of items.
        /// </summary>
        public List<ListItem> items = new List<ListItem>();

        public Color todoColor = new Color(1, 1, 0.50f);
        public Color todoFinishedColor = new Color(1, 1, 0.80f);
        public Color noteColor = new Color(0.15f, 0.15f, 0.15f);
        public Color noteFinishedColor = new Color(0.35f, 0.35f, 0.35f);
        public Color bugColor = new Color(0.498f, 0, 0);
        public Color bugFinishedColor = new Color(0.69f, 0.20f, 0.20f);
        public Color backlogColor = new Color(0.8f, 0, 0);
        public Color backlogFinishedColor = new Color(1f, 0.2f, 0.2f);
        public Color optmizationColor = new Color(0.1f, 0.15f, 0.4f);
        public Color optmizationFinishedColor = new Color(0.3f, 0.35f, 0.6f);
        public Color observationColor = new Color(0.25f, 0.25f, 0.35f);
        public Color observationFinishedColor = new Color(0.45f, 0.45f, 0.55f);
        public Color requestColor = new Color(0.1f, 0.25f, 0.6f);
        public Color requestFinishedColor = new Color(0.3f, 0.45f, 0.8f);
        public Color suggestionColor = new Color(0.4f, 0.05f, 0.5f);
        public Color suggestionFinishedColor = new Color(0.6f, 0.25f, 0.7f);
        public Color inProgressColor = new Color(0.1f, 0.45f, 0.1f);
        /// <summary>
        /// The number of finished items.
        /// </summary>
        public int finishedItems;

        /// <summary>
        /// Constructor for the TODONotesContent class.
        /// </summary>
        public TODONotesContent()
        {
            // Define all our entry types and their colors    
            entryTypes.Add(new ListItemEntryType("TODO", todoColor, todoFinishedColor, 0));
            entryTypes.Add(new ListItemEntryType("Note", noteColor, noteFinishedColor, 1));
            entryTypes.Add(new ListItemEntryType("Bug", bugColor, bugFinishedColor, 2));
            entryTypes.Add(new ListItemEntryType("Backlog", backlogColor, backlogFinishedColor, 3));
            entryTypes.Add(new ListItemEntryType("Optmization", optmizationColor, optmizationFinishedColor, 4));
            entryTypes.Add(new ListItemEntryType("Observation", observationColor, observationFinishedColor, 5));
            entryTypes.Add(new ListItemEntryType("Request", requestColor, requestFinishedColor, 6));
            entryTypes.Add(new ListItemEntryType("Suggestion", suggestionColor, suggestionFinishedColor, 7));
            entryTypes.Add(new ListItemEntryType("In Progress", inProgressColor, inProgressColor, 8));
        }

        /// <summary>
        /// Add a new entry to the list of items.
        /// </summary>
        /// <param name="entryType">The entry type of the new entry.</param>
        /// <param name="entryTitle">The title of the new entry.</param>
        /// <param name="entryContent">The description of the new entry.</param>
        public void AddEntry(ListItemEntryType entryType, string entryTitle, string entryContent)
        {
            ListItem item = new ListItem(entryType, entryTitle, entryContent);
            items.Add(item);
        }
    }

    /// <summary>
    /// A class used to define the contents of each TODO List entry.
    /// </summary>
    [Serializable]
    public class ListItem
    {
        /// <summary>
        /// The type of the entry.
        /// </summary>
        public ListItemEntryType entryType;

        /// <summary>
        /// The title of the entry.
        /// </summary>
        public string entryTitle;

        /// <summary>
        /// The description of the entry.
        /// </summary>
        public string entryContent;

        /// <summary>
        /// A flag indicating whether the entry is finished.
        /// </summary>
        public bool isFinished;

        /// <summary>
        /// A flag indicating whether the entry has been counted as finished.
        /// </summary>
        public bool countedAsFinished;

        /// <summary>
        /// Constructor for the ListItem class.
        /// </summary>
        /// <param name="entryType">The Entry type of the entryContent.</param>
        /// <param name="entryContent">The description of the entryContent.</param>
        public ListItem(ListItemEntryType entryType, string entryTitle, string entryContent)
        {
            this.entryType = entryType;
            this.entryTitle = entryTitle;
            this.entryContent = entryContent;
            isFinished = false;
            countedAsFinished = false;

        }
    }

    /// <summary>
    /// A class used to define the properties of each TODO List entry type.
    /// </summary>
    [Serializable]
    public class ListItemEntryType
    {
        /// <summary>
        /// The name of the entry type.
        /// </summary>
        public string name;

        /// <summary>
        /// The entryColor used to represent the entry type.
        /// </summary>
        public Color entryColor;

        /// <summary>
        /// The entryColor used to represent the entry type once it's completed.
        /// </summary>
        public Color entryFinishedColor;

        /// <summary>
        /// The index of the entry type in the list of entry types.
        /// </summary>
        public int index;

        /// <summary>
        /// Constructor for the ListItemEntryType class.
        /// </summary>
        /// <param name="name">The name of the entry type.</param>
        /// <param name="entryColor">The entryColor used to represent the entry type.</param>
        /// <param name="entryFinishedColor">The entryColor used to represent the entry type once marked as finished.</param>
        /// <param name="index">The index of the entry type in the list of entry types.</param>
        public ListItemEntryType(string name, Color entryColor, Color entryFinishedColor, int index)
        {
            this.name = name;
            this.entryColor = entryColor;
            this.entryFinishedColor = entryFinishedColor;
            this.index = index;
        }
    }
}
#endif
