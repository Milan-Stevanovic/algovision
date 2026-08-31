import { useEffect, useState } from 'react'
import './App.css'
import { createPathfindingConnection } from './services/signalr'

function App() {
  const [connected, setConnected] = useState(false)
  const [message, setMessage] = useState('Waiting for the backend...')

  useEffect(() => {
    const connection = createPathfindingConnection()
    connection.on('Pong', (value: string) => {
      setMessage(value)
    })

    connection.on('ClientConnected', (value: string) => {
      console.log(value)
    })

    void connection.start()
      .then(() => {
        setConnected(true)
        return connection.invoke('Ping')
      })
      .catch((error) => {
        console.error('SignalR connection failed.', error)
        setMessage('Could not connect to the backend.')
      })

    return () => { // cleanup
      connection.off('Pong')
      connection.off('ClientConnected')
      void connection.stop()
    }
  }, [])

  return (
    <main className="page">
      <h1>AlgoVision</h1>
      <p>Connection: {connected ? 'Connected' : 'Disconnected'}</p>
      <p>{message}</p>
    </main>
  )
}

export default App
