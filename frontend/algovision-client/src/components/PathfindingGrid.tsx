import { useState } from "react";
import type { GridPoint, GridTool, VisualCellState } from "../types/pathfinding";


interface Props {
    size: number;
    start: GridPoint | null;
    end: GridPoint | null;
    walls: Set<string>;
    visualization: Record<string, VisualCellState>;
    tool: GridTool;
    disabled: boolean;
    onCellAction: (point: GridPoint) => void;
}

const keyOf = (point: GridPoint) => `${point.x},${point.y}`;
const equals = (first: GridPoint | null, second: GridPoint) =>
    first?.x === second.x && first.y === second.y;

function PathfindingGrid(props: Props) {
    const [drawing, setDrawing] = useState(false);
    const cells = [];

    for (let y = 0; y < props.size; y++) {
        for (let x = 0; x < props.size; x++) {
            const point = { x, y };
            const key = keyOf(point);
            let state: string = props.visualization[key] ?? "empty";
            if (props.walls.has(key)) state = "wall";
            if (equals(props.start, point)) state = "start";
            if (equals(props.end, point)) state = "end";

            cells.push(
                <button
                    className={`grid-cell cell-${state}`}
                    disabled={props.disabled}
                    key={key}
                    onMouseDown={() => {
                        props.onCellAction(point);
                        if (props.tool === "wall" || props.tool === "erase")
                            setDrawing(true);
                    }}
                    onMouseEnter={() => {
                        if (
                            drawing &&
                            (props.tool === "wall" || props.tool === "erase")
                        )
                            props.onCellAction(point);
                    }}
                    type="button"
                >
                    {state === "start" ? "S" : state === "end" ? "E" : ""}
                </button>,
            );
        }
    }

    return (
        <div
            className="pathfinding-grid"
            style={{ gridTemplateColumns: `repeat(${props.size}, 1fr)` }}
            onMouseLeave={() => setDrawing(false)}
            onMouseUp={() => setDrawing(false)}
        >
            {cells}
        </div>
    );
}
export default PathfindingGrid;