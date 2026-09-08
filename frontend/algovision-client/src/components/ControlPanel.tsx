import type { GridTool } from "../types/pathfinding";

interface Props {
    tool: GridTool;
    size: number;
    delay: number;
    running: boolean;
    connected: boolean;
    onToolChange: (tool: GridTool) => void;
    onSizeChange: (size: number) => void;
    onDelayChange: (delay: number) => void;
    onRun: () => void;
    onClearTrace: () => void;
    onClearGrid: () => void;
}

function ControlPanel(props: Props) {
    return (
        <aside className="controls">
            <h1>AlgoVision</h1>
            <p>Breadth-First Search</p>

            <label>
                Grid size
                <select
                    disabled={props.running}
                    value={props.size}
                    onChange={(event) =>
                        props.onSizeChange(Number(event.target.value))
                    }
                >
                    {[15, 20, 25, 30, 40].map((size) => (
                        <option key={size} value={size}>
                            {size} × {size}
                        </option>
                    ))}
                </select>
            </label>

            <div className="tool-list">
                {(["start", "end", "wall", "erase"] as GridTool[]).map(
                    (tool) => (
                        <button
                            className={props.tool === tool ? "active" : ""}
                            disabled={props.running}
                            key={tool}
                            onClick={() => props.onToolChange(tool)}
                            type="button"
                        >
                            {tool}
                        </button>
                    ),
                )}
            </div>

            <label>
                Delay: {props.delay} ms
                <input
                    disabled={props.running}
                    min="5"
                    max="200"
                    step="5"
                    type="range"
                    value={props.delay}
                    onChange={(event) =>
                        props.onDelayChange(Number(event.target.value))
                    }
                />
            </label>

            <button
                disabled={props.running || !props.connected}
                onClick={props.onRun}
                type="button"
            >
                Find Path
            </button>
            <button
                disabled={props.running}
                onClick={props.onClearTrace}
                type="button"
            >
                Clear Visualization
            </button>
            <button
                disabled={props.running}
                onClick={props.onClearGrid}
                type="button"
            >
                Clear Grid
            </button>
        </aside>
    );
}
export default ControlPanel;