using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SpellDatabaseEditorWindow : EditorWindow
{
    private const string DefaultRootFolder = "Assets/SpellSystem/";
    private const string DefaultElementFolder = "Assets/SpellSystem/Elements/";
    private const string DefaultSpellTypeFolder = "Assets/SpellSystem/SpellTypes/";
    private const string DefaultComboFolder = "Assets/SpellSystem/Combos/";

    private const float LeftPanelWidth = 320f;
    private const float RightPanelWidth = 420f;
    private const float MatrixHeaderWidth = 180f;
    private const float MatrixCellWidth = 170f;
    private const float MatrixCellHeight = 70f;
    private const float SmallButtonWidth = 64f;

    private enum SelectionKind
    {
        None,
        Element,
        SpellType,
        Cell
    }

    private enum ButtonStyleKind
    {
        Primary,
        Secondary,
        Warning
    }

    private SpellDatabase database;

    private SelectionKind selectionKind = SelectionKind.None;
    private SpellElementDefinition selectedElement;
    private SpellTypeDefinition selectedSpellType;
    private SpellComboDefinition selectedCombo;

    private ObjectField databaseField;

    private Button saveButton;
    private Button refreshButton;
    private Button cleanButton;

    private ScrollView elementsScrollView;
    private ScrollView spellTypesScrollView;
    private ScrollView matrixScrollView;

    private Label elementsHeaderLabel;
    private Label spellTypesHeaderLabel;
    private Label matrixHeaderLabel;

    private Label inspectorTitleLabel;
    private Label inspectorSubtitleLabel;
    private VisualElement inspectorActionsContainer;
    private VisualElement inspectorMessageContainer;
    private ScrollView inspectorScrollView;
    private VisualElement inspectorContentContainer;

    private Label footerMainLabel;
    private Label footerWarningLabel;

    private SerializedObject activeSerializedObject;

    [MenuItem("Tools/Spells/Spell Database Editor")]
    public static void OpenWindow()
    {
        SpellDatabaseEditorWindow window = GetWindow<SpellDatabaseEditorWindow>();
        window.titleContent = new GUIContent("Spell Database");
        window.minSize = new Vector2(1250f, 700f);
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("Spell Database");
        Undo.undoRedoPerformed += OnExternalChange;
        EditorApplication.projectChanged += OnExternalChange;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnExternalChange;
        EditorApplication.projectChanged -= OnExternalChange;
    }

    private void OnExternalChange()
    {
        if (rootVisualElement != null && rootVisualElement.childCount > 0)
        {
            RefreshAll();
        }
    }

    public void CreateGUI()
    {
        rootVisualElement.Clear();
        BuildWindow();
        RefreshAll();
    }

    private void BuildWindow()
    {
        VisualElement root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;
        root.style.flexGrow = 1;
        root.style.paddingLeft = 8;
        root.style.paddingRight = 8;
        root.style.paddingTop = 8;
        root.style.paddingBottom = 8;
        root.style.backgroundColor = new Color(0.13f, 0.13f, 0.14f);

        root.Add(BuildToolbar());
        root.Add(BuildBody());
        root.Add(BuildFooter());
    }

    private VisualElement BuildToolbar()
    {
        VisualElement toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.alignItems = Align.Center;
        toolbar.style.height = 40f;
        toolbar.style.paddingLeft = 8f;
        toolbar.style.paddingRight = 8f;
        toolbar.style.marginBottom = 8f;
        toolbar.style.backgroundColor = new Color(0.17f, 0.17f, 0.18f);
        SetBorder(toolbar, new Color(0.28f, 0.28f, 0.30f));

        Label title = new Label("Spell Database Editor");
        title.style.width = 170f;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.unityTextAlign = TextAnchor.MiddleLeft;
        title.style.color = Color.white;

        databaseField = new ObjectField("Database");
        databaseField.objectType = typeof(SpellDatabase);
        databaseField.allowSceneObjects = false;
        databaseField.style.minWidth = 360f;
        databaseField.style.marginRight = 8f;
        databaseField.RegisterValueChangedCallback(evt =>
        {
            database = evt.newValue as SpellDatabase;
            ClearSelection();
            RefreshAll();
        });

        Button createDatabaseButton = CreateToolbarButton("New Database", CreateNewDatabase, ButtonStyleKind.Primary);
        saveButton = CreateToolbarButton("Save", SaveAssetsToDisk, ButtonStyleKind.Secondary);
        refreshButton = CreateToolbarButton("Refresh", RefreshAll, ButtonStyleKind.Secondary);
        cleanButton = CreateToolbarButton("Clean Nulls", CleanDatabase, ButtonStyleKind.Warning);

        toolbar.Add(title);
        toolbar.Add(databaseField);
        toolbar.Add(createDatabaseButton);
        toolbar.Add(saveButton);
        toolbar.Add(refreshButton);
        toolbar.Add(cleanButton);

        return toolbar;
    }

    private VisualElement BuildBody()
    {
        VisualElement body = new VisualElement();
        body.style.flexDirection = FlexDirection.Row;
        body.style.flexGrow = 1;

        body.Add(BuildLeftSidebar());
        body.Add(BuildMatrixPanel());
        body.Add(BuildInspectorPanel());

        return body;
    }

    private VisualElement BuildLeftSidebar()
    {
        VisualElement leftPanel = new VisualElement();
        leftPanel.style.width = LeftPanelWidth;
        leftPanel.style.minWidth = LeftPanelWidth;
        leftPanel.style.marginRight = 8f;
        leftPanel.style.flexDirection = FlexDirection.Column;

        leftPanel.Add(BuildElementsSection());
        leftPanel.Add(BuildSpellTypesSection());

        return leftPanel;
    }

    private VisualElement BuildElementsSection()
    {
        VisualElement panel = CreatePanel();
        panel.style.flexGrow = 1;
        panel.style.marginBottom = 8f;

        elementsHeaderLabel = CreateSectionHeader("Elements");
        panel.Add(elementsHeaderLabel);

        VisualElement actionRow = CreateSectionActionRow();

        Button addNewButton = CreateToolbarButton("Add New", CreateNewElement, ButtonStyleKind.Primary);
        addNewButton.style.flexGrow = 1;

        ObjectField addExistingField = new ObjectField();
        addExistingField.objectType = typeof(SpellElementDefinition);
        addExistingField.allowSceneObjects = false;
        addExistingField.style.flexGrow = 1;
        addExistingField.tooltip = "Add existing element asset to database";
        addExistingField.RegisterValueChangedCallback(evt =>
        {
            SpellElementDefinition asset = evt.newValue as SpellElementDefinition;
            if (asset != null)
            {
                AddExistingElement(asset);
                addExistingField.SetValueWithoutNotify(null);
            }
        });

        actionRow.Add(addNewButton);
        actionRow.Add(addExistingField);

        elementsScrollView = new ScrollView();
        elementsScrollView.style.flexGrow = 1;
        elementsScrollView.style.paddingLeft = 6f;
        elementsScrollView.style.paddingRight = 6f;
        elementsScrollView.style.paddingBottom = 6f;

        panel.Add(actionRow);
        panel.Add(elementsScrollView);

        return panel;
    }

    private VisualElement BuildSpellTypesSection()
    {
        VisualElement panel = CreatePanel();
        panel.style.flexGrow = 1;

        spellTypesHeaderLabel = CreateSectionHeader("Spell Types");
        panel.Add(spellTypesHeaderLabel);

        VisualElement actionRow = CreateSectionActionRow();

        Button addNewButton = CreateToolbarButton("Add New", CreateNewSpellType, ButtonStyleKind.Primary);
        addNewButton.style.flexGrow = 1;

        ObjectField addExistingField = new ObjectField();
        addExistingField.objectType = typeof(SpellTypeDefinition);
        addExistingField.allowSceneObjects = false;
        addExistingField.style.flexGrow = 1;
        addExistingField.tooltip = "Add existing spell type asset to database";
        addExistingField.RegisterValueChangedCallback(evt =>
        {
            SpellTypeDefinition asset = evt.newValue as SpellTypeDefinition;
            if (asset != null)
            {
                AddExistingSpellType(asset);
                addExistingField.SetValueWithoutNotify(null);
            }
        });

        actionRow.Add(addNewButton);
        actionRow.Add(addExistingField);

        spellTypesScrollView = new ScrollView();
        spellTypesScrollView.style.flexGrow = 1;
        spellTypesScrollView.style.paddingLeft = 6f;
        spellTypesScrollView.style.paddingRight = 6f;
        spellTypesScrollView.style.paddingBottom = 6f;

        panel.Add(actionRow);
        panel.Add(spellTypesScrollView);

        return panel;
    }

    private VisualElement BuildMatrixPanel()
    {
        VisualElement panel = CreatePanel();
        panel.style.flexGrow = 1;
        panel.style.marginRight = 8f;

        matrixHeaderLabel = CreateSectionHeader("Spell Matrix");
        panel.Add(matrixHeaderLabel);

        matrixScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        matrixScrollView.style.flexGrow = 1;
        matrixScrollView.style.paddingLeft = 6f;
        matrixScrollView.style.paddingRight = 6f;
        matrixScrollView.style.paddingBottom = 6f;

        panel.Add(matrixScrollView);

        return panel;
    }

    private VisualElement BuildInspectorPanel()
    {
        VisualElement panel = CreatePanel();
        panel.style.width = RightPanelWidth;
        panel.style.minWidth = RightPanelWidth;
        panel.style.flexShrink = 0f;

        inspectorTitleLabel = new Label("Inspector");
        inspectorTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        inspectorTitleLabel.style.color = Color.white;
        inspectorTitleLabel.style.fontSize = 14f;
        inspectorTitleLabel.style.marginLeft = 8f;
        inspectorTitleLabel.style.marginTop = 8f;

        inspectorSubtitleLabel = new Label("Select an element, spell type, or matrix cell.");
        inspectorSubtitleLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
        inspectorSubtitleLabel.style.marginLeft = 8f;
        inspectorSubtitleLabel.style.marginRight = 8f;
        inspectorSubtitleLabel.style.marginBottom = 8f;
        inspectorSubtitleLabel.style.whiteSpace = WhiteSpace.Normal;

        inspectorActionsContainer = new VisualElement();
        inspectorActionsContainer.style.flexDirection = FlexDirection.Row;
        inspectorActionsContainer.style.flexWrap = Wrap.Wrap;
        inspectorActionsContainer.style.marginLeft = 8f;
        inspectorActionsContainer.style.marginRight = 8f;
        inspectorActionsContainer.style.marginBottom = 6f;

        inspectorMessageContainer = new VisualElement();
        inspectorMessageContainer.style.flexDirection = FlexDirection.Column;
        inspectorMessageContainer.style.marginLeft = 8f;
        inspectorMessageContainer.style.marginRight = 8f;
        inspectorMessageContainer.style.marginBottom = 6f;

        inspectorScrollView = new ScrollView();
        inspectorScrollView.style.flexGrow = 1;
        inspectorScrollView.style.marginLeft = 8f;
        inspectorScrollView.style.marginRight = 8f;
        inspectorScrollView.style.marginBottom = 8f;

        inspectorContentContainer = new VisualElement();
        inspectorContentContainer.style.flexDirection = FlexDirection.Column;
        inspectorContentContainer.style.flexGrow = 1;

        inspectorScrollView.Add(inspectorContentContainer);

        panel.Add(inspectorTitleLabel);
        panel.Add(inspectorSubtitleLabel);
        panel.Add(inspectorActionsContainer);
        panel.Add(inspectorMessageContainer);
        panel.Add(inspectorScrollView);

        return panel;
    }

    private VisualElement BuildFooter()
    {
        VisualElement footer = new VisualElement();
        footer.style.flexDirection = FlexDirection.Row;
        footer.style.alignItems = Align.Center;
        footer.style.height = 34f;
        footer.style.marginTop = 8f;
        footer.style.paddingLeft = 8f;
        footer.style.paddingRight = 8f;
        footer.style.backgroundColor = new Color(0.17f, 0.17f, 0.18f);
        SetBorder(footer, new Color(0.28f, 0.28f, 0.30f));

        footerMainLabel = new Label("No database selected.");
        footerMainLabel.style.flexGrow = 1;
        footerMainLabel.style.color = Color.white;

        footerWarningLabel = new Label(string.Empty);
        footerWarningLabel.style.color = new Color(1f, 0.85f, 0.40f);

        footer.Add(footerMainLabel);
        footer.Add(footerWarningLabel);

        return footer;
    }

    private void RefreshAll()
    {
        EnsureSelectionIsValid();

        if (databaseField != null)
        {
            databaseField.SetValueWithoutNotify(database);
        }

        RefreshToolbarState();
        RefreshElementList();
        RefreshSpellTypeList();
        RefreshMatrix();
        RefreshInspector();
        RefreshFooter();
    }

    private void RefreshToolbarState()
    {
        bool hasDatabase = database != null;

        if (saveButton != null)
            saveButton.SetEnabled(hasDatabase);

        if (refreshButton != null)
            refreshButton.SetEnabled(true);

        if (cleanButton != null)
            cleanButton.SetEnabled(hasDatabase);
    }

    private void RefreshElementList()
    {
        if (elementsScrollView == null)
            return;

        elementsScrollView.Clear();
        elementsHeaderLabel.text = database == null ? "Elements" : "Elements (" + database.elements.Count + ")";

        if (database == null)
        {
            elementsScrollView.Add(CreateEmptyState("Create or assign a SpellDatabase to begin."));
            return;
        }

        if (database.elements.Count == 0)
        {
            elementsScrollView.Add(
                CreateEmptyState(
                    "No elements yet.\nUse Add New or drag an existing element asset into the field above."));
            return;
        }

        for (int i = 0; i < database.elements.Count; i++)
        {
            SpellElementDefinition element = database.elements[i];
            bool isSelected = selectionKind == SelectionKind.Element && selectedElement == element;
            elementsScrollView.Add(CreateElementRow(element, i, isSelected));
        }
    }

    private void RefreshSpellTypeList()
    {
        if (spellTypesScrollView == null)
            return;

        spellTypesScrollView.Clear();
        spellTypesHeaderLabel.text =
            database == null ? "Spell Types" : "Spell Types (" + database.spellTypes.Count + ")";

        if (database == null)
        {
            spellTypesScrollView.Add(CreateEmptyState("Create or assign a SpellDatabase to begin."));
            return;
        }

        if (database.spellTypes.Count == 0)
        {
            spellTypesScrollView.Add(CreateEmptyState(
                "No spell types yet.\nUse Add New or drag an existing spell type asset into the field above."));
            return;
        }

        for (int i = 0; i < database.spellTypes.Count; i++)
        {
            SpellTypeDefinition spellType = database.spellTypes[i];
            bool isSelected = selectionKind == SelectionKind.SpellType && selectedSpellType == spellType;
            spellTypesScrollView.Add(CreateSpellTypeRow(spellType, i, isSelected));
        }
    }

    private void RefreshMatrix()
    {
        if (matrixScrollView == null)
            return;

        matrixScrollView.Clear();

        if (database == null)
        {
            matrixHeaderLabel.text = "Spell Matrix";
            matrixScrollView.Add(CreateEmptyState("No database selected."));
            return;
        }

        int totalCells = database.GetTotalCellCount();
        matrixHeaderLabel.text = "Spell Matrix (" + totalCells + " cells)";

        if (database.elements.Count == 0 || database.spellTypes.Count == 0)
        {
            matrixScrollView.Add(CreateEmptyState("Add at least one element and one spell type to build the matrix."));
            return;
        }

        VisualElement matrixRoot = new VisualElement();
        matrixRoot.style.flexDirection = FlexDirection.Column;

        VisualElement headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.Add(CreateMatrixHeaderCell("Spell Type \\ Element", MatrixHeaderWidth));

        for (int e = 0; e < database.elements.Count; e++)
        {
            SpellElementDefinition element = database.elements[e];
            headerRow.Add(CreateMatrixHeaderCell(element.name, MatrixCellWidth));
        }

        matrixRoot.Add(headerRow);

        for (int t = 0; t < database.spellTypes.Count; t++)
        {
            SpellTypeDefinition spellType = database.spellTypes[t];

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.Add(CreateMatrixHeaderCell(spellType.name, MatrixHeaderWidth));

            for (int e = 0; e < database.elements.Count; e++)
            {
                SpellElementDefinition element = database.elements[e];
                SpellComboDefinition combo = database.GetCombo(element, spellType);

                bool isSelected = selectionKind == SelectionKind.Cell &&
                                  selectedElement == element &&
                                  selectedSpellType == spellType;

                row.Add(CreateMatrixCell(element, spellType, combo, isSelected));
            }

            matrixRoot.Add(row);
        }

        matrixScrollView.Add(matrixRoot);
    }

    private void RefreshInspector()
    {
        inspectorActionsContainer.Clear();
        inspectorMessageContainer.Clear();
        inspectorContentContainer.Clear();
        activeSerializedObject = null;

        if (database == null)
        {
            inspectorTitleLabel.text = "Inspector";
            inspectorSubtitleLabel.text = "No database selected.";
            inspectorContentContainer.Add(CreateEmptyState("Create or assign a SpellDatabase to start editing."));
            return;
        }

        if (selectionKind == SelectionKind.None)
        {
            inspectorTitleLabel.text = "Inspector";
            inspectorSubtitleLabel.text = "Select an element, spell type, or matrix cell.";
            inspectorContentContainer.Add(CreateEmptyState(
                "Left panel:\nSelect an element or spell type.\n\n" +
                "Center panel:\nClick a matrix cell to create, assign, load, or clear a combo."));
            return;
        }

        if (selectionKind == SelectionKind.Element)
        {
            RefreshElementInspector();
            return;
        }

        if (selectionKind == SelectionKind.SpellType)
        {
            RefreshSpellTypeInspector();
            return;
        }

        if (selectionKind == SelectionKind.Cell)
        {
            RefreshCellInspector();
            return;
        }
    }

    private void RefreshElementInspector()
    {
        if (selectedElement == null)
        {
            ClearSelection();
            RefreshInspector();
            return;
        }

        inspectorTitleLabel.text = "Element";
        inspectorSubtitleLabel.text = selectedElement.name;

        inspectorActionsContainer.Add(CreateActionButton("Ping Asset", () => PingObject(selectedElement),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Select Asset", () => SelectObject(selectedElement),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Remove From Database", () => RemoveElement(selectedElement),
            ButtonStyleKind.Warning));

        inspectorMessageContainer.Add(CreateInfoBox("This tool uses the asset file name as the visible name."));
        inspectorContentContainer.Add(CreateAssetRenameBox(selectedElement));
        BuildInlineInspectorForObject(selectedElement);
    }

    private void RefreshSpellTypeInspector()
    {
        if (selectedSpellType == null)
        {
            ClearSelection();
            RefreshInspector();
            return;
        }

        inspectorTitleLabel.text = "Spell Type";
        inspectorSubtitleLabel.text = selectedSpellType.name;

        inspectorActionsContainer.Add(CreateActionButton("Ping Asset", () => PingObject(selectedSpellType),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Select Asset", () => SelectObject(selectedSpellType),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Remove From Database",
            () => RemoveSpellType(selectedSpellType), ButtonStyleKind.Warning));

        inspectorMessageContainer.Add(CreateInfoBox("This tool uses the asset file name as the visible name."));
        inspectorContentContainer.Add(CreateAssetRenameBox(selectedSpellType));
        BuildInlineInspectorForObject(selectedSpellType);
    }

    private void RefreshCellInspector()
    {
        if (selectedElement == null || selectedSpellType == null)
        {
            ClearSelection();
            RefreshInspector();
            return;
        }

        selectedCombo = database.GetCombo(selectedElement, selectedSpellType);

        inspectorTitleLabel.text = "Matrix Cell";
        inspectorSubtitleLabel.text = selectedSpellType.name + " × " + selectedElement.name;

        inspectorMessageContainer.Add(CreateInfoBox(
            "This cell represents one spell type + one element. " +
            "Names come from the asset file names."));

        VisualElement factsBox = CreateSubBox();
        factsBox.Add(CreateKeyValueRow("Element", selectedElement.name));
        factsBox.Add(CreateKeyValueRow("Spell Type", selectedSpellType.name));
        factsBox.Add(CreateKeyValueRow("Status", selectedCombo == null ? "Empty" : "Assigned"));
        inspectorMessageContainer.Add(factsBox);

        ObjectField comboField = new ObjectField("Assigned Combo");
        comboField.objectType = typeof(SpellComboDefinition);
        comboField.allowSceneObjects = false;
        comboField.value = selectedCombo;
        comboField.tooltip = "Assign an existing combo asset to this cell";
        comboField.RegisterValueChangedCallback(evt =>
        {
            AssignComboToSelectedCell(evt.newValue as SpellComboDefinition);
        });

        inspectorMessageContainer.Add(comboField);

        if (selectedCombo == null)
        {
            inspectorActionsContainer.Add(CreateActionButton("Create New Combo", CreateComboForSelectedCell,
                ButtonStyleKind.Primary));
            inspectorContentContainer.Add(CreateEmptyState("No combo assigned to this cell."));
            return;
        }

        inspectorActionsContainer.Add(CreateActionButton("Create New Combo", CreateComboForSelectedCell,
            ButtonStyleKind.Primary));
        inspectorActionsContainer.Add(CreateActionButton("Ping Asset", () => PingObject(selectedCombo),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Select Asset", () => SelectObject(selectedCombo),
            ButtonStyleKind.Secondary));
        inspectorActionsContainer.Add(CreateActionButton("Clear Link", ClearSelectedCell, ButtonStyleKind.Warning));

        if (selectedCombo.element != selectedElement || selectedCombo.spellType != selectedSpellType)
        {
            inspectorMessageContainer.Add(CreateWarningBox(
                "The assigned combo asset links do not match this cell. " +
                "Assigning from this tool auto-syncs those links."));
        }

        inspectorContentContainer.Add(CreateAssetRenameBox(selectedCombo));
        inspectorContentContainer.Add(CreateSectionSubHeader("Live Combo Inspector"));
        BuildInlineInspectorForObject(selectedCombo);
    }

    private void BuildInlineInspectorForObject(Object targetObject)
    {
        if (targetObject == null)
        {
            inspectorContentContainer.Add(CreateEmptyState("Nothing selected."));
            return;
        }

        activeSerializedObject = new SerializedObject(targetObject);

        InspectorElement inspectorElement = new InspectorElement();
        inspectorElement.Bind(activeSerializedObject);
        inspectorElement.style.flexGrow = 1f;

        inspectorContentContainer.Add(inspectorElement);
    }

    private VisualElement CreateElementRow(SpellElementDefinition element, int index, bool isSelected)
    {
        VisualElement row = CreateListRow(isSelected);

        VisualElement swatch = new VisualElement();
        swatch.style.width = 12f;
        swatch.style.height = 20f;
        swatch.style.marginRight = 6f;
        swatch.style.marginTop = 4f;
        swatch.style.backgroundColor = element != null ? element.elementColor : new Color(0.4f, 0.1f, 0.1f);

        Button selectButton = new Button(() => SelectElement(element));
        selectButton.text = element.name;
        selectButton.style.flexGrow = 1f;
        StyleListSelectButton(selectButton, isSelected);

        Button pingButton = CreateMiniButton("Ping", () => PingObject(element), ButtonStyleKind.Secondary);
        Button removeButton = CreateMiniButton("X", () => RemoveElementAt(index), ButtonStyleKind.Warning);

        row.Add(swatch);
        row.Add(selectButton);
        row.Add(pingButton);
        row.Add(removeButton);

        return row;
    }

    private VisualElement CreateSpellTypeRow(SpellTypeDefinition spellType, int index, bool isSelected)
    {
        VisualElement row = CreateListRow(isSelected);

        Label prefix = new Label(spellType != null ? spellType.deliveryCategory.ToString().Substring(0, 1) : "!");
        prefix.style.width = 14f;
        prefix.style.unityTextAlign = TextAnchor.MiddleCenter;
        prefix.style.marginRight = 6f;
        prefix.style.color = new Color(0.85f, 0.85f, 0.85f);

        Button selectButton = new Button(() => SelectSpellType(spellType));
        selectButton.text = spellType.name;
        selectButton.style.flexGrow = 1f;
        StyleListSelectButton(selectButton, isSelected);

        Button pingButton = CreateMiniButton("Ping", () => PingObject(spellType), ButtonStyleKind.Secondary);
        Button removeButton = CreateMiniButton("X", () => RemoveSpellTypeAt(index), ButtonStyleKind.Warning);

        row.Add(prefix);
        row.Add(selectButton);
        row.Add(pingButton);
        row.Add(removeButton);

        return row;
    }

    private VisualElement CreateMatrixHeaderCell(string text, float width)
    {
        Label cell = new Label(text);
        cell.style.width = width;
        cell.style.height = 30f;
        cell.style.unityTextAlign = TextAnchor.MiddleCenter;
        cell.style.unityFontStyleAndWeight = FontStyle.Bold;
        cell.style.backgroundColor = new Color(0.20f, 0.20f, 0.22f);
        cell.style.color = Color.white;
        cell.style.whiteSpace = WhiteSpace.Normal;
        SetBorder(cell, new Color(0.30f, 0.30f, 0.33f));

        return cell;
    }

    private VisualElement CreateMatrixCell(
        SpellElementDefinition element,
        SpellTypeDefinition spellType,
        SpellComboDefinition combo,
        bool isSelected)
    {
        Button button = new Button(() => SelectCell(element, spellType));

        if (combo == null)
        {
            button.text = "Empty";
            button.tooltip = "No combo assigned";
            button.style.backgroundColor = isSelected
                ? new Color(0.42f, 0.23f, 0.23f)
                : new Color(0.28f, 0.19f, 0.19f);
        }
        else
        {
            button.text = combo.name;
            button.tooltip = "Assigned combo asset: " + combo.name;

            bool invalid = combo.element != element || combo.spellType != spellType;

            if (invalid)
            {
                button.style.backgroundColor = isSelected
                    ? new Color(0.50f, 0.37f, 0.18f)
                    : new Color(0.36f, 0.28f, 0.16f);
            }
            else
            {
                button.style.backgroundColor = isSelected
                    ? new Color(0.22f, 0.42f, 0.58f)
                    : new Color(0.21f, 0.33f, 0.22f);
            }
        }

        button.style.width = MatrixCellWidth;
        button.style.height = MatrixCellHeight;
        button.style.whiteSpace = WhiteSpace.Normal;
        button.style.unityTextAlign = TextAnchor.MiddleCenter;
        button.style.color = Color.white;
        button.style.fontSize = 11f;
        SetBorder(button, new Color(0.30f, 0.30f, 0.33f));

        return button;
    }

    private void SelectElement(SpellElementDefinition element)
    {
        selectedElement = element;
        selectedSpellType = null;
        selectedCombo = null;
        selectionKind = element != null ? SelectionKind.Element : SelectionKind.None;
        RefreshAll();
    }

    private void SelectSpellType(SpellTypeDefinition spellType)
    {
        selectedElement = null;
        selectedSpellType = spellType;
        selectedCombo = null;
        selectionKind = spellType != null ? SelectionKind.SpellType : SelectionKind.None;
        RefreshAll();
    }

    private void SelectCell(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        selectedElement = element;
        selectedSpellType = spellType;
        selectedCombo = database != null ? database.GetCombo(element, spellType) : null;
        selectionKind = (element != null && spellType != null) ? SelectionKind.Cell : SelectionKind.None;
        RefreshAll();
    }

    private void ClearSelection()
    {
        selectionKind = SelectionKind.None;
        selectedElement = null;
        selectedSpellType = null;
        selectedCombo = null;
    }

    private void EnsureSelectionIsValid()
    {
        if (database == null)
        {
            ClearSelection();
            return;
        }

        if (selectionKind == SelectionKind.Element)
        {
            if (selectedElement == null || !database.elements.Contains(selectedElement))
                ClearSelection();
        }
        else if (selectionKind == SelectionKind.SpellType)
        {
            if (selectedSpellType == null || !database.spellTypes.Contains(selectedSpellType))
                ClearSelection();
        }
        else if (selectionKind == SelectionKind.Cell)
        {
            bool validElement = selectedElement != null && database.elements.Contains(selectedElement);
            bool validType = selectedSpellType != null && database.spellTypes.Contains(selectedSpellType);

            if (!validElement || !validType)
                ClearSelection();
        }
    }

    private void CreateNewDatabase()
    {
        string folder = EnsureFolder(DefaultRootFolder);
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + " _SpellDatabase.asset");

        SpellDatabase newDatabase = CreateInstance<SpellDatabase>();
        AssetDatabase.CreateAsset(newDatabase, path);

        database = newDatabase;
        databaseField.SetValueWithoutNotify(database);

        MarkDirty(database);
        SaveAssetsToDisk();

        ClearSelection();
        SelectObject(database);
        RefreshAll();
    }

    private void CreateNewElement()
    {
        if (database == null)
            return;

        string folder = EnsureFolder(DefaultElementFolder);
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "_Element.asset");

        SpellElementDefinition asset = CreateInstance<SpellElementDefinition>();
        AssetDatabase.CreateAsset(asset, path);

        Undo.RecordObject(database, "Add Spell Element");
        database.elements.Add(asset);
        database.Normalize();

        MarkDirty(asset);
        MarkDirty(database);
        SaveAssetsToDisk();

        SelectElement(asset);
        PingObject(asset);
    }

    private void CreateNewSpellType()
    {
        if (database == null)
            return;

        string folder = EnsureFolder(DefaultSpellTypeFolder);
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "_SpellType.asset");

        SpellTypeDefinition asset = CreateInstance<SpellTypeDefinition>();
        AssetDatabase.CreateAsset(asset, path);

        Undo.RecordObject(database, "Add Spell Type");
        database.spellTypes.Add(asset);
        database.Normalize();

        MarkDirty(asset);
        MarkDirty(database);
        SaveAssetsToDisk();

        SelectSpellType(asset);
        PingObject(asset);
    }

    private void AddExistingElement(SpellElementDefinition asset)
    {
        if (database == null || asset == null)
            return;

        Undo.RecordObject(database, "Add Existing Element");

        if (!database.elements.Contains(asset))
            database.elements.Add(asset);

        database.Normalize();

        MarkDirty(database);
        SaveAssetsToDisk();

        SelectElement(asset);
    }

    private void AddExistingSpellType(SpellTypeDefinition asset)
    {
        if (database == null || asset == null)
            return;

        Undo.RecordObject(database, "Add Existing Spell Type");

        if (!database.spellTypes.Contains(asset))
            database.spellTypes.Add(asset);

        database.Normalize();

        MarkDirty(database);
        SaveAssetsToDisk();

        SelectSpellType(asset);
    }

    private void RemoveElementAt(int index)
    {
        if (database == null || index < 0 || index >= database.elements.Count)
            return;

        RemoveElement(database.elements[index]);
    }

    private void RemoveSpellTypeAt(int index)
    {
        if (database == null || index < 0 || index >= database.spellTypes.Count)
            return;

        RemoveSpellType(database.spellTypes[index]);
    }

    private void RemoveElement(SpellElementDefinition asset)
    {
        if (database == null || asset == null)
            return;

        Undo.RecordObject(database, "Remove Spell Element");
        database.elements.Remove(asset);
        database.Normalize();

        MarkDirty(database);
        SaveAssetsToDisk();

        if (selectedElement == asset)
            ClearSelection();

        RefreshAll();
    }

    private void RemoveSpellType(SpellTypeDefinition asset)
    {
        if (database == null || asset == null)
            return;

        Undo.RecordObject(database, "Remove Spell Type");
        database.spellTypes.Remove(asset);
        database.Normalize();

        MarkDirty(database);
        SaveAssetsToDisk();

        if (selectedSpellType == asset)
            ClearSelection();

        RefreshAll();
    }

    private void CreateComboForSelectedCell()
    {
        if (database == null || selectionKind != SelectionKind.Cell || selectedElement == null ||
            selectedSpellType == null)
            return;

        SpellComboDefinition combo = CreateNewComboAsset(selectedElement, selectedSpellType);
        AssignComboToSelectedCell(combo);
    }

    private SpellComboDefinition CreateNewComboAsset(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        string folder = EnsureFolder(DefaultComboFolder);
        string comboAssetName = GetSuggestedComboAssetName(element, spellType);
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + comboAssetName);

        SpellComboDefinition combo = CreateInstance<SpellComboDefinition>();
        combo.element = element;
        combo.spellType = spellType;

        AssetDatabase.CreateAsset(combo, path);

        MarkDirty(combo);
        SaveAssetsToDisk();

        return combo;
    }

    private void AssignComboToSelectedCell(SpellComboDefinition combo)
    {
        if (database == null || selectionKind != SelectionKind.Cell || selectedElement == null ||
            selectedSpellType == null)
            return;

        Undo.RecordObject(database, "Assign Spell Combo");

        if (combo == null)
        {
            database.RemoveComboLink(selectedElement, selectedSpellType);
            MarkDirty(database);
            SaveAssetsToDisk();

            selectedCombo = null;
            RefreshAll();
            return;
        }

        Undo.RecordObject(combo, "Sync Combo Links");
        combo.element = selectedElement;
        combo.spellType = selectedSpellType;
        RenameAssetSilently(combo, GetSuggestedComboAssetName(selectedElement, selectedSpellType));

        database.SetCombo(selectedElement, selectedSpellType, combo);
        database.Normalize();

        MarkDirty(combo);
        MarkDirty(database);
        SaveAssetsToDisk();

        selectedCombo = combo;
        RefreshAll();
    }

    private void ClearSelectedCell()
    {
        if (database == null || selectionKind != SelectionKind.Cell || selectedElement == null ||
            selectedSpellType == null)
            return;

        Undo.RecordObject(database, "Clear Spell Combo Link");
        database.RemoveComboLink(selectedElement, selectedSpellType);

        MarkDirty(database);
        SaveAssetsToDisk();

        selectedCombo = null;
        RefreshAll();
    }

    private void CleanDatabase()
    {
        if (database == null)
            return;

        Undo.RecordObject(database, "Clean Spell Database");
        database.Normalize();

        MarkDirty(database);
        SaveAssetsToDisk();

        RefreshAll();
    }

    private void SaveAssetsToDisk()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshFooter();
    }

    private void RefreshFooter()
    {
        if (database == null)
        {
            footerMainLabel.text = "No database selected.";
            footerWarningLabel.text = string.Empty;
            return;
        }

        int elementsCount = database.elements.Count;
        int typesCount = database.spellTypes.Count;
        int totalCells = database.GetTotalCellCount();
        int assigned = database.GetAssignedCellCount();
        int empty = database.GetEmptyCellCount();
        int invalid = database.GetInvalidComboReferenceCount();

        footerMainLabel.text =
            "Elements: " + elementsCount +
            " | Types: " + typesCount +
            " | Cells: " + totalCells +
            " | Assigned: " + assigned +
            " | Empty: " + empty;

        footerWarningLabel.text = invalid > 0
            ? invalid + " invalid combo reference(s)"
            : "All combo links valid";
    }

    private VisualElement CreateAssetRenameBox(Object targetObject)
    {
        VisualElement box = CreateSubBox();

        Label title = new Label("Asset Name");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.color = Color.white;
        title.style.marginBottom = 6f;

        TextField nameField = new TextField("Name");
        nameField.value = targetObject != null ? targetObject.name : string.Empty;

        Button renameButton = CreateActionButton("Rename Asset", () =>
        {
            if (targetObject != null)
            {
                RenameAsset(targetObject, nameField.value);
            }
        }, ButtonStyleKind.Primary);

        box.Add(title);
        box.Add(nameField);
        box.Add(renameButton);

        return box;
    }

    private void RenameAsset(Object targetObject, string newName)
    {
        if (targetObject == null)
            return;

        RenameAssetSilently(targetObject, newName);
        SaveAssetsToDisk();
        RefreshAll();
    }

    private void RenameAssetSilently(Object targetObject, string newName)
    {
        if (!targetObject)
            return;

        string path = AssetDatabase.GetAssetPath(targetObject);
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (targetObject.name == newName)
            return;

        AssetDatabase.RenameAsset(path, newName);
    }

    private string GetSuggestedComboAssetName(SpellElementDefinition element, SpellTypeDefinition spellType)
    {
        string elementName = element.name;
        string spellTypeName = spellType.name;
        return "Combo" + elementName + "_" + spellTypeName + ".asset";
    }

    private void SelectObject(Object obj)
    {
        if (obj == null)
            return;

        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }

    private void PingObject(Object obj)
    {
        if (obj == null)
            return;

        EditorGUIUtility.PingObject(obj);
    }

    private void MarkDirty(Object obj)
    {
        if (obj == null)
            return;

        EditorUtility.SetDirty(obj);
    }

    private string EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }

        return folderPath;
    }

    private string CombineAssetPath(string a, string b)
    {
        return (a + "/" + b).Replace("\\", "/");
    }


    private VisualElement CreatePanel()
    {
        VisualElement panel = new VisualElement();
        panel.style.flexDirection = FlexDirection.Column;
        panel.style.backgroundColor = new Color(0.18f, 0.18f, 0.19f);
        SetBorder(panel, new Color(0.30f, 0.30f, 0.33f));
        return panel;
    }

    private Label CreateSectionHeader(string text)
    {
        Label header = new Label(text);
        header.style.height = 30f;
        header.style.unityTextAlign = TextAnchor.MiddleLeft;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.color = Color.white;
        header.style.backgroundColor = new Color(0.21f, 0.21f, 0.23f);
        header.style.paddingLeft = 8f;
        header.style.marginBottom = 6f;
        return header;
    }

    private Label CreateSectionSubHeader(string text)
    {
        Label header = new Label(text);
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.color = Color.white;
        header.style.marginBottom = 6f;
        header.style.marginTop = 2f;
        return header;
    }

    private VisualElement CreateSectionActionRow()
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.marginLeft = 6f;
        row.style.marginRight = 6f;
        row.style.marginBottom = 6f;
        return row;
    }

    private VisualElement CreateListRow(bool isSelected)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.paddingLeft = 6f;
        row.style.paddingRight = 6f;
        row.style.paddingTop = 4f;
        row.style.paddingBottom = 4f;
        row.style.marginBottom = 4f;
        row.style.backgroundColor = isSelected
            ? new Color(0.22f, 0.31f, 0.42f)
            : new Color(0.20f, 0.20f, 0.21f);

        SetBorder(row, new Color(0.30f, 0.30f, 0.33f));
        return row;
    }

    private void StyleListSelectButton(Button button, bool isSelected)
    {
        button.style.flexGrow = 1f;
        button.style.color = Color.white;
        button.style.unityTextAlign = TextAnchor.MiddleLeft;
        button.style.backgroundColor = isSelected
            ? new Color(0.26f, 0.36f, 0.48f)
            : new Color(0.25f, 0.25f, 0.27f);
        button.style.marginRight = 4f;
        button.style.whiteSpace = WhiteSpace.Normal;

        SetBorder(button, new Color(0.35f, 0.35f, 0.38f));
    }

    private Button CreateToolbarButton(string text, System.Action onClick, ButtonStyleKind kind)
    {
        Button button = new Button(() => onClick());
        button.text = text;
        button.style.height = 26f;
        button.style.marginRight = 6f;

        StyleButton(button, kind);
        return button;
    }

    private Button CreateActionButton(string text, System.Action onClick, ButtonStyleKind kind)
    {
        Button button = new Button(() => onClick());
        button.text = text;
        button.style.height = 24f;
        button.style.marginRight = 6f;
        button.style.marginBottom = 6f;

        StyleButton(button, kind);
        return button;
    }

    private Button CreateMiniButton(string text, System.Action onClick, ButtonStyleKind kind)
    {
        Button button = new Button(() => onClick());
        button.text = text;
        button.style.width = SmallButtonWidth;
        button.style.height = 22f;
        button.style.marginLeft = 4f;

        StyleButton(button, kind);
        return button;
    }

    private void StyleButton(Button button, ButtonStyleKind kind)
    {
        button.style.color = Color.white;
        button.style.unityTextAlign = TextAnchor.MiddleCenter;

        if (kind == ButtonStyleKind.Primary)
        {
            button.style.backgroundColor = new Color(0.22f, 0.42f, 0.58f);
        }
        else if (kind == ButtonStyleKind.Warning)
        {
            button.style.backgroundColor = new Color(0.47f, 0.24f, 0.21f);
        }
        else
        {
            button.style.backgroundColor = new Color(0.28f, 0.28f, 0.30f);
        }

        SetBorder(button, new Color(0.35f, 0.35f, 0.38f));
    }

    private VisualElement CreateEmptyState(string text)
    {
        Label label = new Label(text);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.color = new Color(0.85f, 0.85f, 0.85f);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.paddingLeft = 12f;
        label.style.paddingRight = 12f;
        label.style.paddingTop = 16f;
        label.style.paddingBottom = 16f;
        label.style.marginTop = 8f;
        label.style.backgroundColor = new Color(0.20f, 0.20f, 0.21f);

        SetBorder(label, new Color(0.30f, 0.30f, 0.33f));
        return label;
    }

    private VisualElement CreateInfoBox(string text)
    {
        Label label = new Label(text);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.color = Color.white;
        label.style.paddingLeft = 8f;
        label.style.paddingRight = 8f;
        label.style.paddingTop = 6f;
        label.style.paddingBottom = 6f;
        label.style.backgroundColor = new Color(0.20f, 0.29f, 0.38f);
        label.style.marginBottom = 6f;

        SetBorder(label, new Color(0.32f, 0.40f, 0.49f));
        return label;
    }

    private VisualElement CreateWarningBox(string text)
    {
        Label label = new Label(text);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.color = Color.white;
        label.style.paddingLeft = 8f;
        label.style.paddingRight = 8f;
        label.style.paddingTop = 6f;
        label.style.paddingBottom = 6f;
        label.style.backgroundColor = new Color(0.38f, 0.30f, 0.14f);
        label.style.marginBottom = 6f;

        SetBorder(label, new Color(0.48f, 0.39f, 0.21f));
        return label;
    }

    private VisualElement CreateSubBox()
    {
        VisualElement box = new VisualElement();
        box.style.flexDirection = FlexDirection.Column;
        box.style.paddingLeft = 8f;
        box.style.paddingRight = 8f;
        box.style.paddingTop = 8f;
        box.style.paddingBottom = 8f;
        box.style.backgroundColor = new Color(0.20f, 0.20f, 0.21f);
        box.style.marginBottom = 6f;

        SetBorder(box, new Color(0.30f, 0.30f, 0.33f));
        return box;
    }

    private VisualElement CreateKeyValueRow(string key, string value)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.marginBottom = 2f;

        Label keyLabel = new Label(key + ":");
        keyLabel.style.width = 90f;
        keyLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
        keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

        Label valueLabel = new Label(value);
        valueLabel.style.flexGrow = 1f;
        valueLabel.style.color = Color.white;
        valueLabel.style.whiteSpace = WhiteSpace.Normal;

        row.Add(keyLabel);
        row.Add(valueLabel);

        return row;
    }

    private void SetBorder(VisualElement element, Color color)
    {
        element.style.borderTopWidth = 1f;
        element.style.borderBottomWidth = 1f;
        element.style.borderLeftWidth = 1f;
        element.style.borderRightWidth = 1f;

        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }
}