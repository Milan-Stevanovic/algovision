import type { GridTool } from "../types/pathfinding";
import type { AlgorithmKey } from "../types/pathfinding";

interface Props {
    algorithm: AlgorithmKey;
    tool: GridTool;
    size: number;
    delay: number;
    running: boolean;
    connected: boolean;
    onAlgorithmChange: (algorithm: AlgorithmKey) => void;
    onToolChange: (tool: GridTool) => void;
    onSizeChange: (size: number) => void;
    onDelayChange: (delay: number) => void;
    onRun: () => void;
    onStop: () => void;
    onClearTrace: () => void;
    onClearGrid: () => void;
}

function ControlPanel(props: Props) {
    return (
        <aside className="controls">
            <h1>AlgoVision</h1>
            
            <label>Algorithm</label>
            <select disabled={props.running} value={props.algorithm} onChange={(event) => props.onAlgorithmChange(event.target.value as AlgorithmKey)}>
                <option value="bfs">Breadth-First Search</option>
                <option value="dfs">Depth-First Search</option>
                <option value="dijkstra">Dijkstra's Algorithm</option>
                <option value="astar">A* Algorithm</option>
                <option value="bidirectional-search">Bidirectional Search</option>
            </select>

            <label>
                Grid size
                <select
                    disabled={props.running}
                    value={props.size}
                    onChange={(event) =>
                        props.onSizeChange(Number(event.target.value))
                    }
                >
                    {[15, 20, 25, 30].map((size) => (
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
                            {tool.toUpperCase()}
                        </button>
                    ),
                )}
            </div>

            <label>
                Delay: {props.delay} ms
                <input
                    disabled={props.running}
                    min="0"
                    max="500"
                    step="5"
                    type="range"
                    value={props.delay}
                    onChange={(event) =>
                        props.onDelayChange(Number(event.target.value))
                    }
                />
            </label>

            <button disabled={props.running || !props.connected} onClick={props.onRun} type="button">
                Find Path
            </button>
            <button disabled={!props.running} onClick={props.onStop} type="button">
                Stop
            </button>
            <button disabled={props.running} onClick={props.onClearTrace} type="button">
                Clear Visualization
            </button>
            <button disabled={props.running} onClick={props.onClearGrid} type="button">
                Clear Grid
            </button>
        </aside>
    );
}
export default ControlPanel;