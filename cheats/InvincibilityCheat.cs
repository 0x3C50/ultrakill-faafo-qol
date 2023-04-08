namespace ExtraCommands.cheats;

public class InvincibilityCheat : ICheat
{
    public void Enable()
    {
        Plugin.TakeDamage = false;
    }

    public void Disable()
    {
        Plugin.TakeDamage = true;
    }

    public void Update()
    {
    }

    public string LongName => "Invincibility";
    public string Identifier => "extras.cheats.invincibility";
    public string ButtonEnabledOverride => null;
    public string ButtonDisabledOverride => null;
    public string Icon => "main";
    public bool IsActive => !Plugin.TakeDamage;
    public bool DefaultState => false;
    public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;
}