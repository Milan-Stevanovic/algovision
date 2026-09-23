import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

const apiUrl = 'https://localhost:5001'

export const createPathfindingConnection = () => new HubConnectionBuilder()
  .withUrl(`${apiUrl}/pathfindingHub`)
  .withAutomaticReconnect()
  .configureLogging(LogLevel.Warning)
  .build()
