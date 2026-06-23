
using Cysharp.Threading.Tasks;

public interface ISaveManager
{
    public void LoadAllSlots();
    public bool IsEmpty(int slot);
    public SaveMeta GetMeta(int slot);
    public void Save();
    public UniTask Load(int slot);
    public void StartNewGame(int slot);
}