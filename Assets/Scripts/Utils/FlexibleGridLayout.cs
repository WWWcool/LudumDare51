using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public enum FitType
    {
        Uniform,
        Width,
        Height
    }

    public class FlexibleGridLayout : LayoutGroup
    {
        [SerializeField] private FitType fitType;
        [SerializeField] private int fixedRows;
        [SerializeField] private int fixedColumns;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private bool squareAspectRatio;

        public Vector2 CellSize => cellSize;
        public Vector2Int GridSize => new Vector2Int(fixedColumns, fixedRows);

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            var pad = padding;
            var padX = 0f;
            var padY = 0f;
            float sqrRt = Mathf.Sqrt(rectChildren.Count);
            var rows = Mathf.CeilToInt(sqrRt);
            var columns = Mathf.CeilToInt(sqrRt);

            switch (fitType)
            {
                case FitType.Width:
                    columns = fixedColumns;
                    rows = Mathf.CeilToInt(rectChildren.Count / (float) columns);
                    break;
                case FitType.Height:
                    rows = fixedRows;
                    rows = Mathf.CeilToInt(rectChildren.Count / (float) rows);
                    break;
                case FitType.Uniform:
                    rows = fixedRows;
                    columns = fixedColumns;
                    break;
            }

            var rect = ((RectTransform) rectTransform).rect;
            float parentWidth = rect.width;
            float parentHeight = rect.height;

            float cellWidth = (parentWidth / (float) columns) - ((spacing.x / ((float) columns)) * (columns - 1)) -
                              (pad.left / (float) columns) - (pad.right / (float) columns);
            float cellHeight = (parentHeight / (float) rows) - ((spacing.y / ((float) rows)) * (rows - 1)) -
                               (pad.top / (float) rows) - (pad.bottom / (float) rows);

            if (fitType == FitType.Width && squareAspectRatio)
            {
                cellHeight = cellWidth;
            }

            if (fitType == FitType.Height && squareAspectRatio)
            {
                cellWidth = cellHeight;
            }

            if (squareAspectRatio)
            {
                if (cellHeight < cellWidth)
                {
                    padX = cellWidth * columns - cellHeight * columns;
                    cellWidth = cellHeight;
                }
                else
                {
                    padY = cellHeight * rows - cellWidth * rows;
                    cellHeight = cellWidth;
                }
            }

            cellSize.x = cellWidth;
            cellSize.y = cellHeight;

            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + pad.left;
                var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + pad.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }

            var xSize = (cellSize.x * columns) + (spacing.x * columns) + pad.left + pad.right;
            var ySize = (cellSize.y * rows) + (spacing.y * rows) + pad.top + pad.bottom;

            // rectTransform.anchoredPosition = new Vector2(padX / 4, padY / 4);

            SetLayoutInputForAxis(xSize, xSize, xSize, 0);
            SetLayoutInputForAxis(ySize, ySize, ySize, 1);
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}