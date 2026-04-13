using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Flags]
public enum ItemMoveModifier
{
    None = 0,
    Alt = 1 << 1,
    Shift = 1 << 2,
    Ctrl = 1 << 3,
    BackpackOpen = 1 << 4,
}
