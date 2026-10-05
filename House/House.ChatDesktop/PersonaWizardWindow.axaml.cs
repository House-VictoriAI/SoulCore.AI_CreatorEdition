using Avalonia.Controls;
using Avalonia.Interactivity;
using House.ChatDesktop.Services;

namespace House.ChatDesktop;

public partial class PersonaWizardWindow : Window
{
    private readonly SoulCorePersonaClient _client;
    private readonly bool _isCreate;
    private readonly PersonaPackInfo? _editTarget;
    private IReadOnlyList<PersonaPackInfo> _hostPacks;

    private int _step;
    private PersonaTemplateInfo? _selectedTemplate;
    private bool _personaIdTouched;
    private bool _suggestingPersonaId;
    private bool _saving;
    private bool _retrying;
    private bool _loadingModels;
    private bool _bindingModelCombo;
    private string? _selectedInferenceModel;
    private string? _hostDefaultModel;
    private string? _resolvedForThisPack;

    public PersonaPackInfo? SavedPersona { get; private set; }
    public string? ResultNotice { get; private set; }

    /// <summary>Avalonia / design-time loader entry.</summary>
    public PersonaWizardWindow()
        : this(new SoulCorePersonaClient(), Array.Empty<PersonaPackInfo>(), editTarget: null)
    {
    }

    public PersonaWizardWindow(
        SoulCorePersonaClient client,
        IReadOnlyList<PersonaPackInfo> hostPacks,
        PersonaPackInfo? editTarget = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hostPacks = hostPacks ?? Array.Empty<PersonaPackInfo>();
        _editTarget = editTarget;
        _isCreate = editTarget is null;

        InitializeComponent();

        Title = _isCreate ? "Create persona" : "Edit persona";
        TitleText.Text = Title;

        WarmthSlider.PropertyChanged += (_, _) => RefreshBandPreview();
        DirectnessSlider.PropertyChanged += (_, _) => RefreshBandPreview();
        FormalitySlider.PropertyChanged += (_, _) => RefreshBandPreview();
        PlayfulnessSlider.PropertyChanged += (_, _) => RefreshBandPreview();
        BoundarySlider.PropertyChanged += (_, _) => RefreshBandPreview();
        RecallSlider.PropertyChanged += (_, _) => RefreshBandPreview();

        PersonaIdBox.TextChanged += (_, _) =>
        {
            if (!_suggestingPersonaId)
                _personaIdTouched = true;
        };

        if (_isCreate)
        {
            BindHostTemplates();
            _selectedInferenceModel = null;
            _resolvedForThisPack = null;
            _step = 0;
        }
        else
        {
            LoadEditTarget(_editTarget!);
            _step = 1;
        }

        BindModelPicker(Array.Empty<string>());
        ShowStep();
        _ = LoadInferenceModelsAsync();
    }

    private void BindHostTemplates()
    {
        var templates = PersonaWizardLogic.BuildTemplates(_hostPacks);
        TemplateList.ItemsSource = templates;
        TemplateList.SelectedItem = templates.FirstOrDefault();
        _selectedTemplate = TemplateList.SelectedItem as PersonaTemplateInfo;

        var blocked = templates.Count == 0;
        HostRequiredText.IsVisible = blocked;
        RetryTemplatesButton.IsVisible = blocked;
        TemplateList.IsVisible = !blocked;
        if (blocked)
            StatusText.Text = PersonaWizardLogic.HostGalleryRequiredMessage;
    }

    private void LoadEditTarget(PersonaPackInfo pack)
    {
        DisplayNameBox.Text = pack.DisplayName;
        PersonaIdBox.Text = pack.PersonaId;
        PersonaIdBox.IsReadOnly = true;
        HumanAddressBox.Text = pack.HumanAddress ?? "Friend";
        IdentityBlurbBox.Text = pack.IdentityBlurb ?? "";
        _selectedInferenceModel = PersonaWizardLogic.NormalizeInferenceModel(pack.InferenceModel);
        _resolvedForThisPack = PersonaWizardLogic.NormalizeInferenceModel(pack.ResolvedInferenceModel);
        ApplyTraits(pack.Traits ?? new PersonaTraitScalesInfo());
        // Prefer Host compiledDirectives on edit load (PROP-15.10 F3).
        if (BandPreviewText is not null)
        {
            BandPreviewText.Text = PersonaWizardLogic.PreferHostOrBandPreview(
                pack.CompiledDirectives,
                pack.Traits ?? new PersonaTraitScalesInfo());
        }

        _personaIdTouched = true;
        RefreshModelHint();
    }

    private async Task LoadInferenceModelsAsync()
    {
        if (_loadingModels)
            return;

        _loadingModels = true;
        try
        {
            var snap = await _client.ListInferenceModelsAsync().ConfigureAwait(true);
            _hostDefaultModel = snap.HostModel;
            // Prefer pack resolved (GET /api/personas); else models probe resolvedInferenceModel / hostModel.
            if (string.IsNullOrWhiteSpace(_resolvedForThisPack))
            {
                _resolvedForThisPack = PersonaWizardLogic.NormalizeInferenceModel(_selectedInferenceModel)
                                       ?? snap.ResolvedInferenceModel
                                       ?? snap.HostModel;
            }

            BindModelPicker(snap.Reachable ? snap.Models : Array.Empty<string>());
            if (!snap.Reachable && InferenceModelHintText is not null)
            {
                InferenceModelHintText.Text =
                    "Model list unavailable (Host unreachable). Blank still saves as Host default.";
            }
        }
        finally
        {
            _loadingModels = false;
        }
    }

    private void BindModelPicker(IReadOnlyList<string> models)
    {
        if (InferenceModelCombo is null)
            return;

        var options = PersonaWizardLogic.BuildModelPickerOptions(
            models,
            _selectedInferenceModel,
            _hostDefaultModel);
        _bindingModelCombo = true;
        try
        {
            InferenceModelCombo.ItemsSource = options;
            InferenceModelCombo.SelectedItem =
                PersonaWizardLogic.FindModelOption(options, _selectedInferenceModel);
        }
        finally
        {
            _bindingModelCombo = false;
        }

        RefreshModelHint();
    }

    private void InferenceModelCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_bindingModelCombo)
            return;

        if (InferenceModelCombo.SelectedItem is InferenceModelOption opt)
            _selectedInferenceModel = PersonaWizardLogic.NormalizeInferenceModel(opt.ModelId);
        else
            _selectedInferenceModel = null;

        // Live hint: override shows itself; blank inherits Host default from models probe.
        _resolvedForThisPack = _selectedInferenceModel
                               ?? _hostDefaultModel
                               ?? _resolvedForThisPack;
        RefreshModelHint();
    }

    private void RefreshModelHint()
    {
        if (InferenceModelHintText is null)
            return;
        InferenceModelHintText.Text = PersonaWizardLogic.FormatModelResolutionHint(
            _selectedInferenceModel,
            _resolvedForThisPack,
            _hostDefaultModel);
    }

    private void TemplateList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedTemplate = TemplateList.SelectedItem as PersonaTemplateInfo;
    }

    private async void RetryTemplates_Click(object? sender, RoutedEventArgs e)
    {
        if (_retrying || !_isCreate)
            return;

        _retrying = true;
        StatusText.Text = "Loading Host packs…";
        RetryTemplatesButton.IsEnabled = false;
        try
        {
            var list = await _client.ListAsync().ConfigureAwait(true);
            if (!list.Reachable)
            {
                StatusText.Text = list.Detail ?? PersonaWizardLogic.HostGalleryRequiredMessage;
                return;
            }

            _hostPacks = list.Personas;
            BindHostTemplates();
            if (PersonaWizardLogic.BuildTemplates(_hostPacks).Count > 0)
                StatusText.Text = "";
        }
        finally
        {
            _retrying = false;
            RetryTemplatesButton.IsEnabled = true;
        }
    }

    private void DisplayNameBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isCreate || _personaIdTouched || PersonaIdBox.IsReadOnly)
            return;
        _suggestingPersonaId = true;
        try
        {
            PersonaIdBox.Text = PersonaWizardLogic.SuggestPersonaId(DisplayNameBox.Text);
        }
        finally
        {
            _suggestingPersonaId = false;
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(false);

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        if (_saving)
            return;
        if (_step <= (_isCreate ? 0 : 1))
            return;
        _step--;
        ShowStep();
    }

    private async void Next_Click(object? sender, RoutedEventArgs e)
    {
        if (_saving)
            return;

        if (_step == 0)
        {
            if (TemplateList.SelectedItem is not PersonaTemplateInfo template)
            {
                StatusText.Text = PersonaWizardLogic.BuildTemplates(_hostPacks).Count == 0
                    ? PersonaWizardLogic.HostGalleryRequiredMessage
                    : "Select a Host template (Blank / Mentor / Analyst when seeded).";
                return;
            }

            _selectedTemplate = template;
            ApplyTemplateDefaults(template);
            _step = 1;
            ShowStep();
            return;
        }

        if (_step == 1)
        {
            if (!ValidateIdentity(out var error))
            {
                StatusText.Text = error;
                return;
            }

            _step = 2;
            ShowStep();
            return;
        }

        if (_step == 2)
        {
            RefreshBandPreview();
            _step = 3;
            ShowStep();
        }

        await Task.CompletedTask.ConfigureAwait(true);
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (_saving)
            return;
        if (!ValidateIdentity(out var error))
        {
            StatusText.Text = error;
            return;
        }

        _saving = true;
        StatusText.Text = "Saving to Host…";
        SaveButton.IsEnabled = false;
        NextButton.IsEnabled = false;
        BackButton.IsEnabled = false;

        try
        {
            var traits = ReadTraits();
            PersonaWriteSnapshot result;
            if (_isCreate)
            {
                var templates = PersonaWizardLogic.BuildTemplates(_hostPacks);
                var template = _selectedTemplate ?? templates.FirstOrDefault();
                if (template is null)
                {
                    StatusText.Text = PersonaWizardLogic.HostGalleryRequiredMessage;
                    return;
                }

                var write = PersonaWizardLogic.BuildCreateWrite(
                    template,
                    PersonaIdBox.Text ?? "",
                    DisplayNameBox.Text ?? "",
                    HumanAddressBox.Text ?? "Friend",
                    IdentityBlurbBox.Text,
                    traits,
                    _selectedInferenceModel);
                result = await _client.CreateAsync(write).ConfigureAwait(true);
            }
            else
            {
                var edits = new PersonaPackWrite
                {
                    PersonaId = _editTarget!.PersonaId,
                    DisplayName = DisplayNameBox.Text ?? "",
                    HumanAddress = HumanAddressBox.Text ?? "Friend",
                    IdentityBlurb = IdentityBlurbBox.Text,
                    InferenceModel = _selectedInferenceModel,
                    Traits = traits
                };
                var write = PersonaWizardLogic.BuildUpdateWrite(_editTarget, edits);
                result = await _client.UpdateAsync(_editTarget.PersonaId, write).ConfigureAwait(true);
            }

            if (!result.Ok || result.Persona is null)
            {
                StatusText.Text = result.Detail ?? "Host rejected save";
                return;
            }

            SavedPersona = result.Persona;
            ResultNotice = PersonaWizardLogic.BuildSaveNextTurnNotice(result.Persona.DisplayName, _isCreate);
            Close(true);
        }
        finally
        {
            _saving = false;
            SaveButton.IsEnabled = true;
            NextButton.IsEnabled = true;
            BackButton.IsEnabled = true;
        }
    }

    private void ApplyTemplateDefaults(PersonaTemplateInfo template)
    {
        if (string.IsNullOrWhiteSpace(DisplayNameBox.Text))
            DisplayNameBox.Text = "";
        HumanAddressBox.Text = template.HumanAddress;
        IdentityBlurbBox.Text = template.IdentityBlurb ?? "";
        ApplyTraits(template.Traits);
        _personaIdTouched = false;
        if (!string.IsNullOrWhiteSpace(DisplayNameBox.Text))
        {
            _suggestingPersonaId = true;
            try
            {
                PersonaIdBox.Text = PersonaWizardLogic.SuggestPersonaId(DisplayNameBox.Text);
            }
            finally
            {
                _suggestingPersonaId = false;
            }
        }
    }

    private void ApplyTraits(PersonaTraitScalesInfo traits)
    {
        WarmthSlider.Value = traits.Warmth;
        DirectnessSlider.Value = traits.Directness;
        FormalitySlider.Value = traits.Formality;
        PlayfulnessSlider.Value = traits.Playfulness;
        BoundarySlider.Value = traits.BoundaryStrictness;
        RecallSlider.Value = traits.RecallBias;
        RefreshBandPreview();
    }

    private PersonaTraitScalesInfo ReadTraits() => new()
    {
        Warmth = WarmthSlider.Value,
        Directness = DirectnessSlider.Value,
        Formality = FormalitySlider.Value,
        Playfulness = PlayfulnessSlider.Value,
        BoundaryStrictness = BoundarySlider.Value,
        RecallBias = RecallSlider.Value
    };

    private void RefreshBandPreview()
    {
        if (BandPreviewText is null)
            return;
        // Live slider: local 0.34/0.67 mirror (must match PersonaTraitCompiler).
        BandPreviewText.Text = PersonaWizardLogic.FormatBandPreview(ReadTraits());
    }

    private bool ValidateIdentity(out string error)
    {
        var name = (DisplayNameBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Display name is required.";
            return false;
        }

        var id = (PersonaIdBox.Text ?? "").Trim().ToLowerInvariant();
        if (_isCreate)
        {
            PersonaIdBox.Text = id;
            if (!PersonaWizardLogic.IsValidPersonaId(id))
            {
                error = "Persona id must be lowercase letters, digits, '-' or '_' (1–64).";
                return false;
            }

            if (_hostPacks.Any(p => string.Equals(p.PersonaId, id, StringComparison.OrdinalIgnoreCase)))
            {
                error = $"Persona id '{id}' already exists on Host.";
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(HumanAddressBox.Text))
            HumanAddressBox.Text = "Friend";

        error = "";
        return true;
    }

    private void ShowStep()
    {
        TemplateStep.IsVisible = _isCreate && _step == 0;
        IdentityStep.IsVisible = _step == 1;
        TraitsStep.IsVisible = _step == 2;
        CharterStep.IsVisible = _step == 3;

        BackButton.IsVisible = _step > (_isCreate ? 0 : 1);
        NextButton.IsVisible = _step < 3;
        SaveButton.IsVisible = _step == 3;

        if (_isCreate && _step == 0 && PersonaWizardLogic.BuildTemplates(_hostPacks).Count == 0)
            NextButton.IsEnabled = false;
        else if (!_saving)
            NextButton.IsEnabled = true;

        StepHintText.Text = _step switch
        {
            0 => "Step 1 — pick a Host template (Blank / Mentor / Analyst when seeded).",
            1 => _isCreate
                ? "Step 2 — name, Host id, human address, and optional chat model."
                : "Edit name, human address, and chat model for this pack (id is fixed).",
            2 => "Step 3 — trait scales (compiled to Low/Mid/High bands on Host).",
            3 => "Step 4 — charter / identity seed + quarantine notice.",
            _ => ""
        };

        if (_step == 1)
            RefreshModelHint();

        if (_step == 3)
        {
            var name = (DisplayNameBox.Text ?? "").Trim();
            var id = (PersonaIdBox.Text ?? "").Trim();
            QuarantineNoticeText.Text = _isCreate
                ? PersonaWizardLogic.BuildCreateQuarantineNotice(name, id)
                : PersonaQuarantineConfirm.BuildEditQuarantineNotice(name, id);
        }

        if (_step == 2)
            RefreshBandPreview();

        if (!(_isCreate && _step == 0 && PersonaWizardLogic.BuildTemplates(_hostPacks).Count == 0))
            StatusText.Text = "";
    }
}
