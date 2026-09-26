using CapybaraGame.Core;
using CapybaraGame.Services;

namespace CapybaraGame.Characters
{
    public sealed class CharacterSystem
    {
        public CharacterId Active { get; private set; }

        public CharacterSystem(LocalSave save)
        {
            Active = (CharacterId)save.activeCharacter;
        }

        public void Select(CharacterId id, LocalSave save)
        {
            Active = id;
            save.activeCharacter = (int)id;
            SaveService.Save(save);
        }
    }
}
