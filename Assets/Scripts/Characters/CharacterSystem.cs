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

        public bool IsOwned(CharacterId id, LocalSave save)
        {
            return save != null && save.ownedCharacters != null && save.ownedCharacters.Contains((int)id);
        }

        public void Select(CharacterId id, LocalSave save)
        {
            if (!IsOwned(id, save)) return;
            Active = id;
            save.activeCharacter = (int)id;
            SaveService.Save(save);
        }
    }
}
