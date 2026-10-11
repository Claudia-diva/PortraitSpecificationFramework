using BepInEx.Configuration;
using YKF;
using UnityEngine;

namespace SpecificPortraits.UI
{
    public class TabSettings : YKLayout<object>
    {
        void Toggle(YKVertical column, string label, ConfigEntry<bool> entry)
        {
            YKGrid row = column.Grid();
            row.Layout.cellSize = new Vector2(520f, 40f);
            row.Layout.constraintCount = 1;
            row.Toggle(label, entry.Value, isOn => entry.Value = isOn);
        }

        public override void OnLayout()
        {
            YKVertical column = Vertical();
            column.Layout.spacing = 6f;

            column.Text("ポートレート"._("Portraits"));
            Toggle(column, "常にバニラポートレートを含める"._("Always include vanilla portraits"), SpecificPortraits.AlwaysIncludeVanillaPortraits);
            Toggle(column, "一致するプールがない場合はランダムに選ぶ"._("Random portrait when no pool matches"), SpecificPortraits.RandomizePortraitOnNoResults);
            Toggle(column, "「ポートレートの再ロール」ダイアログオプションを有効にする"._("Enable \"Reroll Portrait\" dialog option"), SpecificPortraits.EnableRerollDialog);

            column.Text("セレクター"._("Picker"));
            Toggle(column, "手動で選んだポートレートをロックする"._("Lock the character when a portrait is picked by hand"), SpecificPortraits.LockOnPick);
            Toggle(column, "マップ再抽選はパーティを飛ばす"._("Reroll Entire Map skips party members"), SpecificPortraits.RerollMapSkipsParty);
            Toggle(column, "マップ再抽選はペットを飛ばす"._("Reroll Entire Map skips pets"), SpecificPortraits.RerollMapSkipsPets);

            column.Text("ウィンドウ"._("Window"));
            Toggle(column, "スクロール位置カウンターを表示"._("Show the on-screen portrait counter"), SpecificPortraits.ShowScrollCounter);
            Toggle(column, "マウスオーバーで名前を表示"._("Show names on hover"), SpecificPortraits.ShowHoverNames);
            Toggle(column, "キャラクター名ボタンを表示"._("Show character name buttons"), SpecificPortraits.ShowCharacterNames);
            column.Text(("列数・セル幅・間隔・ウィンドウサイズは Mod Options または BepInEx の設定ファイルで変更できます。")._(
                "Columns, cell width, spacing and window size can be changed in Mod Options or the BepInEx config file. Grids apply them live; the window size applies on reopen."));
        }
    }
}
