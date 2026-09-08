import { useEffect, useRef, useState } from "react";
import "./App.css";
import { createPathfindingConnection } from "./services/signalr";
import type { GridPoint, GridTool, PathfindingResult, PathfindingStep, VisualCellState } from "./types/pathfinding";
import ControlPanel from "./components/ControlPanel";
import PathfindingGrid from "./components/PathfindingGrid";
import { HubConnectionState } from "@microsoft/signalr";

const keyOf = (point: GridPoint) => `${point.x},${point.y}`;
const equals = (first: GridPoint | null, second: GridPoint) => first?.x === second.x && first.y === second.y;
function App() {
    // grid
    const [size, setSize] = useState(20);
    const [start, setStart] = useState<GridPoint | null>({ x: 2, y: 10 });
    const [end, setEnd] = useState<GridPoint | null>({ x: 17, y: 10 });
    const [walls, setWalls] = useState<Set<string>>(new Set());
    
    //control panel
    const [tool, setTool] = useState<GridTool>("wall");
    const [delay, setDelay] = useState(30);
    
    const [visualization, setVisualization] = useState<Record<string, VisualCellState>>({});
    const [running, setRunning] = useState(false);
    const [message, setMessage] = useState("Draw walls and run BFS.");
    const [connected, setConnected] = useState(false);
    const connectionRef = useRef<ReturnType<typeof createPathfindingConnection> | null>(null);

    useEffect(() => {
        const connection = createPathfindingConnection();
        connectionRef.current = connection;

        connection.on('ClientConnected', (value: string) => {
          console.log(value)
        });

        connection.on("StepReceived", (step: PathfindingStep) => {
            setVisualization((current) => ({
                ...current,
                [keyOf(step)]: step.type.toLowerCase() as VisualCellState,
            }));
            // console.log(step);
        });

        connection.on("PathfindingCompleted", (result: PathfindingResult) => {
            setRunning(false);
            setMessage(
                result.found
                    ? `Path length: ${result.pathLength}. Visualization time: ${result.elapsedTime}.`
                    : "No path found.",
            );
            // console.log(result);
        });

        void connection
            .start()
            .then(() => setConnected(true))
            .catch((error) => {
                setMessage("Backend disconnected.")
                console.error("SignalR connection failed.", error)
            });

        return () => {
            connection.off("ClientConnected");
            connection.off("StepReceived");
            connection.off("PathfindingCompleted");
            connectionRef.current = null;
            void connection.stop();
        };
    }, []);

    function applyTool(point: GridPoint) {
        const key = keyOf(point);
        if (tool === "start" && !equals(end, point)) {
            setStart(point);
            removeWall(key);
        }
        if (tool === "end" && !equals(start, point)) {
            setEnd(point);
            removeWall(key);
        }
        if (tool === "wall" && !equals(start, point) && !equals(end, point))
            setWalls((old) => new Set(old).add(key));
        if (tool === "erase") removeWall(key);
    }

    function removeWall(key: string) {
        setWalls((old) => {
            const next = new Set(old);
            next.delete(key);
            return next;
        });
    }

    function run() {
        const connection = connectionRef.current;
        if (
            !start ||
            !end ||
            !connection ||
            connection.state !== HubConnectionState.Connected
        )
            return;
        const wallPoints = [...walls].map((key) => {
            const [x, y] = key.split(",").map(Number);
            return { x, y };
        });
        setVisualization({});
        setRunning(true);
        void connection
            .invoke("StartPathfinding", {
                gridWidth: size,
                gridHeight: size,
                start,
                end,
                walls: wallPoints,
                algorithm: "bfs",
                animationDelayMs: delay,
            })
            .catch(() => {
                setRunning(false);
                setMessage("Could not run BFS.");
            });
    }
    
    function resize(nextSize: number) {
        setSize(nextSize);
        setStart(null);
        setEnd(null);
        setWalls(new Set());
        setVisualization({});
    }

    return (
        <div className="app-shell">
            <ControlPanel
                tool={tool}
                size={size}
                delay={delay}
                running={running}
                connected={connected}
                onToolChange={setTool}
                onSizeChange={resize}
                onDelayChange={setDelay}
                onRun={run}
                onClearTrace={() => setVisualization({})}
                onClearGrid={() => {
                    setStart(null);
                    setEnd(null);
                    setWalls(new Set());
                    setVisualization({});
                }}
            />
            <main className="workspace">
                <p>{message}</p>
                <PathfindingGrid
                    size={size}
                    start={start}
                    end={end}
                    walls={walls}
                    visualization={visualization}
                    tool={tool}
                    disabled={running}
                    onCellAction={applyTool}
                />
            </main>
        </div>
    );
}

export default App;