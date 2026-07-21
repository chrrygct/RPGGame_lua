using System.Collections;
using System.Collections.Generic;

namespace RPG.Stats
{
    //这是接口。作用是提供一个统一的方式来获取属性的加成和百分比加成。
    public interface IModifierProvider
    {
        IEnumerable<float> GetAdditiveModifier(Stat stat);
        IEnumerable<float> GetPercentageModifier(Stat stat);
    }
}