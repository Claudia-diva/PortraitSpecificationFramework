using UnityEngine;
using UnityEngine.UI;

namespace SpecificPortraits.UI
{
    /// <summary>
    /// A small label in the top-right corner of a scrolling grid: "13-24 / 892", the cells
    /// currently on screen out of all active cells. Updated only when the scroll position,
    /// the grid size or the cell count changes.
    /// </summary>
    public class ScrollCounter : MonoBehaviour
    {
        public ScrollRect Scroll;
        public GridLayoutGroup Grid;
        public Text Label;

        float lastY = float.NaN;
        int lastCount = -1;
        int lastColumns = -1;
        float lastCellHeight = -1f;
        float lastViewHeight = -1f;

        public static ScrollCounter Attach(ScrollRect scroll, GridLayoutGroup grid, Text label)
        {
            var c = scroll.gameObject.AddComponent<ScrollCounter>();
            c.Scroll = scroll;
            c.Grid = grid;
            c.Label = label;
            var viewport = scroll.viewport != null ? scroll.viewport : scroll.content.parent as RectTransform;
            var rt = label.rectTransform;
            rt.SetParent(viewport, false);
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-8f, -4f);
            rt.sizeDelta = new Vector2(220f, 24f);
            label.alignment = TextAnchor.UpperRight;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            rt.SetAsLastSibling();
            return c;
        }

        void LateUpdate()
        {
            if (Scroll == null || Grid == null || Label == null || Scroll.content == null)
                return;
            if (!SpecificPortraits.ShowScrollCounter.Value)
            {
                if (Label.enabled)
                    Label.enabled = false;
                return;
            }
            if (!Label.enabled)
                Label.enabled = true;

            int count = 0;
            var t = Grid.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                if (t.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            float y = Scroll.content.anchoredPosition.y;
            var viewport = Scroll.viewport != null ? Scroll.viewport : Scroll.content.parent as RectTransform;
            float viewHeight = viewport != null ? viewport.rect.height : 0f;
            int columns = Mathf.Max(1, Grid.constraintCount);
            float rowHeight = Grid.cellSize.y + Grid.spacing.y;
            if (rowHeight <= 0f || viewHeight <= 0f)
                return;

            if (count == lastCount && columns == lastColumns && Mathf.Approximately(y, lastY)
                && Mathf.Approximately(rowHeight, lastCellHeight) && Mathf.Approximately(viewHeight, lastViewHeight))
                return;
            lastCount = count;
            lastColumns = columns;
            lastY = y;
            lastCellHeight = rowHeight;
            lastViewHeight = viewHeight;

            if (count == 0)
            {
                Label.text = "0 / 0";
                return;
            }
            // the grid sits below the scroll content's top padding and any header text
            float gridTop = -(Grid.transform as RectTransform).anchoredPosition.y;
            float scrolled = Mathf.Max(0f, y - gridTop);
            int firstRow = Mathf.FloorToInt(scrolled / rowHeight);
            int lastRow = Mathf.FloorToInt((scrolled + viewHeight - 1f) / rowHeight);
            int first = Mathf.Clamp(firstRow * columns + 1, 1, count);
            int last = Mathf.Clamp((lastRow + 1) * columns, first, count);
            Label.text = $"{first}-{last} / {count}";
        }
    }
}
