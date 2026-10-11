using UnityEngine;
using UnityEngine.UI;

namespace SpecificPortraits.UI
{
    /// <summary>
    /// Sizes a grid from the width it actually gets at runtime instead of fixed numbers:
    /// as many columns as fit the configured minimum cell width (or the configured column
    /// count), cells stretched to fill the row, re-evaluated whenever the measured width
    /// changes (resolution, UI scale, resizing).
    /// </summary>
    public class FitGrid : MonoBehaviour
    {
        public GridLayoutGroup Grid;
        /// <summary>Whose width to fill. Usually the scroll viewport; otherwise the grid's parent.</summary>
        public RectTransform WidthSource;
        /// <summary>Column count to force, 0 to derive it from the configured minimum cell width.</summary>
        public int FixedColumns = 0;
        /// <summary>Cell height as a multiple of cell width. Ignored when FixedHeight is set.</summary>
        public float Aspect = 4f / 3f;
        /// <summary>Absolute cell height, 0 to use Aspect.</summary>
        public float FixedHeight = 0f;
        /// <summary>Added below the image, for cards that carry a caption.</summary>
        public float ExtraHeight = 0f;
        /// <summary>Read the column count, minimum cell width and spacing from the mod settings.</summary>
        public bool UseSettings = true;

        float lastWidth = -1f;
        int lastColumns = -1;
        int lastMinWidth = -1;
        int lastSpacing = -1;

        public static FitGrid Attach(GridLayoutGroup grid, RectTransform widthSource)
        {
            var fit = grid.gameObject.AddComponent<FitGrid>();
            fit.Grid = grid;
            fit.WidthSource = widthSource != null ? widthSource : grid.transform.parent as RectTransform;
            return fit;
        }

        void LateUpdate()
        {
            if (Grid == null || WidthSource == null)
                return;
            float width = WidthSource.rect.width;
            if (width <= 0f)
                return;

            int columns = FixedColumns > 0 ? FixedColumns : (UseSettings ? SpecificPortraits.GridColumns.Value : 0);
            int minWidth = UseSettings ? SpecificPortraits.MinCellWidth.Value : 120;
            int spacing = UseSettings ? SpecificPortraits.GridSpacing.Value : (int)Grid.spacing.x;

            if (Mathf.Approximately(width, lastWidth) && columns == lastColumns && minWidth == lastMinWidth && spacing == lastSpacing)
                return;
            lastWidth = width;
            lastColumns = columns;
            lastMinWidth = minWidth;
            lastSpacing = spacing;
            Apply(width, columns, minWidth, spacing);
        }

        void Apply(float width, int columns, int minWidth, int spacing)
        {
            float available = width - Grid.padding.horizontal;
            // the scroll content usually carries its own padding around the grid
            var parentGroup = Grid.transform.parent != null ? Grid.transform.parent.GetComponent<LayoutGroup>() : null;
            if (parentGroup != null)
                available -= parentGroup.padding.horizontal;
            if (available <= 0f)
                return;

            if (UseSettings)
                Grid.spacing = new Vector2(spacing, spacing);
            float gap = Grid.spacing.x;
            if (columns <= 0)
                columns = Mathf.Max(1, Mathf.FloorToInt((available + gap) / (minWidth + gap)));
            float cellWidth = Mathf.Floor((available - (columns - 1) * gap) / columns);
            if (cellWidth < 1f)
                return;
            float cellHeight = (FixedHeight > 0f ? FixedHeight : Mathf.Floor(cellWidth * Aspect)) + ExtraHeight;

            Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            Grid.constraintCount = columns;
            Grid.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}
