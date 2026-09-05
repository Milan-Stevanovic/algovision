export interface GridPoint { x: number; y: number }

export interface PathfindingRequest {
  gridWidth: number
  gridHeight: number
  start: GridPoint
  end: GridPoint
  walls: GridPoint[]
  algorithm: 'bfs'
  animationDelayMs: string
}

export interface PathfindingStep {
  x: number
  y: number
  type: 'Frontier' | 'Visited' | 'Path'
}

export interface PathfindingResult {
  found: boolean
  pathLength: number
  visitedNodes: number
  elapsedTime: string
}