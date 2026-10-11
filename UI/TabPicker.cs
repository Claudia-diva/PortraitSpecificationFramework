using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YKF;

namespace SpecificPortraits.UI
{
    /// <summary>Assign, reroll or lock the portrait of any character on the current map.</summary>
    public class TabPicker : YKLayout<object>
    {
        public const float CaptionHeight = 40f;

        UIText selectedCharaText = null;
        UIText hoverText = null;
        Chara selectedChara = null;
        readonly Dictionary<Chara, Image> charaImages = new Dictionary<Chara, Image>();
        readonly List<KeyValuePair<GameObject, string>> charaCards = new List<KeyValuePair<GameObject, string>>();
        readonly List<KeyValuePair<GameObject, string>> portraitCards = new List<KeyValuePair<GameObject, string>>();

        static string Prompt => "ポートレート交換のためにキャラクターを選んでください"._("Please select a character for portrait swap");
        static string LockedLabel => "（ロック中）"._("(locked)");

        static Sprite CharaSprite(Chara chara)
        {
            var id = chara.GetIdPortrait();
            if (id.IsEmpty())
                return chara.GetSprite();
            return ThumbnailCache.Get(id) ?? SpecificPortraits.GetSprite(id) ?? chara.GetSprite();
        }

        void RefreshImage(Chara chara)
        {
            if (chara != null && charaImages.TryGetValue(chara, out var image) && image != null)
            {
                image.sprite = CharaSprite(chara);
                image.enabled = image.sprite != null;
            }
        }

        void UpdateSelectedText()
        {
            if (selectedCharaText == null)
                return;
            if (selectedChara == null)
                selectedCharaText.text = Prompt;
            else
                selectedCharaText.text = selectedChara.Name + (PortraitPatch.IsLocked(selectedChara) ? " " + LockedLabel : "")
                    + (selectedChara.GetIdPortrait().IsEmpty() ? "" : "  [" + selectedChara.GetIdPortrait() + "]");
        }

        void Select(Chara chara)
        {
            if (selectedChara != null && charaImages.TryGetValue(selectedChara, out var previous) && previous != null)
                previous.color = Color.white;

            selectedChara = chara;
            if (charaImages.TryGetValue(chara, out var image) && image != null)
                image.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            UpdateSelectedText();
        }

        /// <summary>Shared hover line; honours the ShowHoverNames setting.</summary>
        public static void ShowHover(UIText target, string label)
        {
            if (target == null)
                return;
            target.text = SpecificPortraits.ShowHoverNames.Value ? (label ?? "") : "";
        }

        /// <summary>Give a layout-driven control a fixed width so the next control in the row stays visible.</summary>
        public static T FixedWidth<T>(T c, float width) where T : Component
        {
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.flexibleWidth = 0f;
            return c;
        }

        /// <summary>An empty, fixed-width gap inside a horizontal row.</summary>
        public static void Gap(YKHorizontal row, float width)
        {
            var go = new GameObject("gap", typeof(RectTransform));
            go.transform.SetParent(row.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.flexibleWidth = 0f;
        }

        /// <summary>A label + input pair with even spacing. Labels keep their natural width.</summary>
        public static UIInputText SearchField(YKHorizontal row, string label)
        {
            var text = row.Text(label);
            var tle = text.gameObject.AddComponent<LayoutElement>();
            tle.flexibleWidth = 0f;
            tle.preferredWidth = text.preferredWidth + 8f;
            Gap(row, 6f);
            return FixedWidth(row.InputText(""), 300f);
        }

        /// <summary>Hide grid cards whose search key does not contain the text.</summary>
        public static void ApplyFilter(List<KeyValuePair<GameObject, string>> cards, string text)
        {
            var q = (text ?? "").Trim().ToLowerInvariant();
            foreach (var kv in cards)
            {
                if (kv.Key != null)
                    kv.Key.SetActive(q.Length == 0 || kv.Value.Contains(q));
            }
        }

        /// <summary>
        /// A portrait grid inside a scroll view: columns and cell size follow the settings and
        /// the viewport width, plus an on-screen counter in the corner.
        /// </summary>
        public static YKGrid PortraitGrid(YKScroll scroll, float extraHeight = 0f)
        {
            YKGrid grid = scroll.Grid();
            grid.transform.SetParent(scroll.ContentTransform);
            var viewport = scroll.ScrollRect != null && scroll.ScrollRect.viewport != null
                ? scroll.ScrollRect.viewport
                : scroll.ContentTransform.parent as RectTransform;
            var fit = FitGrid.Attach(grid.Layout, viewport);
            fit.ExtraHeight = extraHeight;
            if (scroll.ScrollRect != null)
            {
                var counter = scroll.Text("");
                ScrollCounter.Attach(scroll.ScrollRect, grid.Layout, counter);
            }
            return grid;
        }

        /// <summary>Two panes side by side that share the width of the tab.</summary>
        public static YKGrid PaneGrid(YKLayout tab, float paneHeight)
        {
            YKGrid grid = tab.Grid();
            grid.Layout.spacing = new Vector2(10f, 0f);
            var fit = FitGrid.Attach(grid.Layout, tab.transform as RectTransform);
            fit.UseSettings = false;
            fit.FixedColumns = 2;
            fit.FixedHeight = paneHeight;
            return grid;
        }

        bool RerollMapIncludes(Chara c)
        {
            if (c.IsPC || PortraitPatch.IsLocked(c))
                return false;
            if (SpecificPortraits.RerollMapSkipsParty.Value && c.IsPCParty)
                return false;
            if (SpecificPortraits.RerollMapSkipsPets.Value && c.IsPCFactionMinion)
                return false;
            return true;
        }

        public override void OnLayout()
        {
            var map = SpecificPortraits.CurrentMap;
            if (map == null)
            {
                Text("ゲームをロードしてから開いてください"._("Open this tool while a game is loaded."));
                return;
            }

            YKGrid topGrid = Grid();
            topGrid.Layout.cellSize = new Vector2(320f, 40f);
            topGrid.Layout.spacing = new Vector2(8f, 4f);
            topGrid.Layout.constraintCount = 4;
            topGrid.Layout.childAlignment = TextAnchor.MiddleLeft;

            topGrid.Button("マップ全体を再抽選する"._("Reroll Entire Map"), () =>
            {
                foreach (var chara in map.charas.Where(RerollMapIncludes).ToList())
                {
                    PortraitPatch.Assign(chara, chara.bio, force: false);
                    RefreshImage(chara);
                }
                UpdateSelectedText();
            });
            topGrid.Button("選択を再ロール"._("Reroll Selection"), () =>
            {
                if (selectedChara != null)
                {
                    PortraitPatch.SetLocked(selectedChara, false);
                    PortraitPatch.Assign(selectedChara, selectedChara.bio, force: true);
                    RefreshImage(selectedChara);
                    UpdateSelectedText();
                }
            });
            topGrid.Button("選択をロック／解除"._("Lock / Unlock Selection"), () =>
            {
                if (selectedChara != null)
                {
                    PortraitPatch.SetLocked(selectedChara, !PortraitPatch.IsLocked(selectedChara));
                    UpdateSelectedText();
                }
            });
            topGrid.Button("ポートレートをリセット"._("Reset Portrait"), () =>
            {
                if (selectedChara != null)
                {
                    PortraitPatch.ClearManual(selectedChara);
                    RefreshImage(selectedChara);
                    UpdateSelectedText();
                }
            });

            // Search boxes in their own row, evenly spaced.
            YKHorizontal searchRow = Horizontal();
            searchRow.Layout.spacing = 8f;
            searchRow.Layout.childAlignment = TextAnchor.MiddleLeft;
            searchRow.Layout.childForceExpandWidth = false;
            var charaSearch = SearchField(searchRow, "キャラクター検索"._("Find character"));
            charaSearch.field.onValueChanged.AddListener(v => ApplyFilter(charaCards, v));
            Gap(searchRow, 30f);
            var portraitSearch = SearchField(searchRow, "ポートレート検索"._("Find portrait"));
            portraitSearch.field.onValueChanged.AddListener(v => ApplyFilter(portraitCards, v));

            hoverText = Text(" ");

            YKGrid gridLayoutMain = PaneGrid(this, Mathf.Max(300f, SpecificPortraits.WindowHeight.Value - 180f));

            #region character list
            YKScroll scrollLayoutOriginal = gridLayoutMain.Scroll();
            selectedCharaText = scrollLayoutOriginal.Text(Prompt);
            bool captions = SpecificPortraits.ShowCharacterNames.Value;
            YKGrid gridLayout = PortraitGrid(scrollLayoutOriginal, captions ? CaptionHeight : 0f);

            foreach (var chara in map.charas.ToList())
            {
                var charaCardGO = new GameObject("chara card", typeof(RectTransform));
                charaCardGO.transform.SetParent(gridLayout.transform, false);
                var vlg = charaCardGO.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 2f;
                charaCards.Add(new KeyValuePair<GameObject, string>(charaCardGO, (chara.Name + " " + chara.id).ToLowerInvariant()));
                string hoverLabel = chara.Name + (chara.GetIdPortrait().IsEmpty() ? "" : "  [" + chara.GetIdPortrait() + "]");
                try
                {
                    var imageGO = new GameObject($"{chara.uid} IGO", typeof(RectTransform));
                    imageGO.transform.SetParent(charaCardGO.transform, false);
                    var image = imageGO.AddComponent<Image>();
                    image.preserveAspect = true;
                    image.sprite = CharaSprite(chara);
                    image.enabled = image.sprite != null;
                    var le = imageGO.AddComponent<LayoutElement>();
                    le.flexibleHeight = 1f;
                    charaImages[chara] = image;
                    var button = imageGO.AddComponent<Button>();
                    button.onClick.AddListener(() => Select(chara));
                    HoverLabel.Attach(imageGO, hoverLabel, label => ShowHover(hoverText, label));
                }
                catch (Exception ex)
                {
                    SpecificPortraits.Log.LogWarning($"Could not show {chara.id}: {ex.Message}");
                }
                if (captions)
                {
                    var charaCardButton = Button(chara.Name, () => Select(chara));
                    charaCardButton.transform.SetParent(charaCardGO.transform, false);
                    var ble = charaCardButton.gameObject.GetComponent<LayoutElement>() ?? charaCardButton.gameObject.AddComponent<LayoutElement>();
                    ble.preferredHeight = CaptionHeight - 4f;
                    ble.flexibleHeight = 0f;
                    var caption = charaCardButton.GetComponentInChildren<Text>();
                    if (caption != null)
                    {
                        caption.horizontalOverflow = HorizontalWrapMode.Wrap;
                        caption.verticalOverflow = VerticalWrapMode.Truncate;
                        caption.resizeTextForBestFit = true;
                        caption.resizeTextMinSize = 9;
                        caption.resizeTextMaxSize = Mathf.Max(10, caption.fontSize);
                    }
                }
            }
            #endregion

            #region replacement picker
            YKScroll scrollLayoutReplacement = gridLayoutMain.Scroll();
            YKGrid gridLayoutReplacement = PortraitGrid(scrollLayoutReplacement);

            var pending = new List<KeyValuePair<Image, string>>();
            foreach (var sprite in Portrait.modPortraits.list)
            {
                if (sprite.id == null)
                    continue;
                var id = sprite.id;
                // One flat Image per cell: no wrapper, no per-cell layout group, so the grid stays cheap to rebuild.
                var imageGO = new GameObject($"{id} IGO", typeof(RectTransform));
                imageGO.transform.SetParent(gridLayoutReplacement.transform, false);
                portraitCards.Add(new KeyValuePair<GameObject, string>(imageGO, id.ToLowerInvariant()));
                var image = imageGO.AddComponent<Image>();
                image.preserveAspect = true;
                image.enabled = false;
                pending.Add(new KeyValuePair<Image, string>(image, id));

                var button = imageGO.AddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    if (selectedChara != null)
                    {
                        PortraitPatch.AssignManual(selectedChara, id);
                        RefreshImage(selectedChara);
                        UpdateSelectedText();
                    }
                });
                HoverLabel.Attach(imageGO, id, label => ShowHover(hoverText, label));
            }
            SpriteBatchLoader.Start(this, pending);
            #endregion
        }
    }
}
