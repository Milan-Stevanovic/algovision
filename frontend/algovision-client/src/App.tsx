import { HubConnectionState } from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import "./App.css";
import { createPathfindingConnection } from "./services/signalr";
import type { PathfindingResult, PathfindingStep } from "./types/pathfinding";

function App() {
    const [connected, setConnected] = useState(false);
    const [stepCount, setStepCount] = useState(0);
    const [result, setResult] = useState<PathfindingResult | null>(null);
    const connectionRef = useRef<ReturnType<typeof createPathfindingConnection> | null>(null);

    useEffect(() => {
        const connection = createPathfindingConnection();
        connectionRef.current = connection;

        connection.on("StepReceived", (_step: PathfindingStep) => {
            setStepCount((value) => value + 1);
        });

        connection.on("PathfindingCompleted", (value: PathfindingResult) => {
            setResult(value);
            console.log(value);
        });

            connection.on('ClientConnected', (value: string) => {
          console.log(value)
        });


        void connection
            .start()
            .then(() => setConnected(true))
            .catch((error) =>
                console.error("SignalR connection failed.", error),
            );

        return () => {
            connection.off("StepReceived");
            connection.off("PathfindingCompleted");
            connectionRef.current = null;
            void connection.stop();
        };
    }, []);

    function runSample() {
        const connection = connectionRef.current;
        if (!connection || connection.state !== HubConnectionState.Connected)
            return;
        setStepCount(0);
        setResult(null);
        void connection.invoke("StartPathfinding", {
            gridWidth: 10,
            gridHeight: 10,
            start: { x: 1, y: 1 },
            end: { x: 8, y: 8 },
            walls: [
                { x: 4, y: 4 },
                { x: 4, y: 5 },
            ],
            algorithm: "bfs",
            animationDelayMs: 10,
        });
    }

    return (
        <main className="page">
            <h1>AlgoVision BFS prototype</h1>
            <p>Connection: {connected ? "Connected" : "Disconnected"}</p>
            <button disabled={!connected} onClick={runSample} type="button">
                Run sample BFS
            </button>
            <p>Received steps: {stepCount}</p>
            {result && (
                <p>
                    Found: {String(result.found)}, path length:{" "}
                    {result.pathLength}, elapsed time: {result.elapsedTime}
                </p>
            )}
        </main>
    );
}

export default App;
