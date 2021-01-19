import { CarSimulation } from './CarSimulation'

const car_AI_update = (carSimulation: CarSimulation) => {
    Object.keys(carSimulation.actions).forEach((key) => {
        if (key === 'acceleration') {
            carSimulation.actions[key] = true
        }
    })
}

export { car_AI_update }