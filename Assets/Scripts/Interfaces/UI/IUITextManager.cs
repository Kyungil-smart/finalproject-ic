using Unity.VisualScripting;

public interface IUITextManager
{
    public void ApplyAllText();
    public void ChangeCurrentLanguage(LanguageType language);
    public bool IsDataUpdated { get; set; }
}