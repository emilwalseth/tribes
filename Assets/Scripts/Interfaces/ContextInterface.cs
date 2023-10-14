using System.Collections.Generic;
using Characters;
using Tiles;
using UI;
using UnityEngine;

namespace Interfaces
{
    public interface IContextInterface
    {
        string GetLabel();
        Sprite GetIcon();
        List<ContextButtonData> GetContextButtons();

    }
}
