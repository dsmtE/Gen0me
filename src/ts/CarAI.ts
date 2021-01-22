import { CarSimulation } from './CarSimulation'

interface DistancesToObstacles {
    front: number,
    left: number,
    right: number,
}

class ActivationParams {
    private c1: number
    private c2: number
    private c3: number
    private bias: number

    constructor() {
        this.c1   = Math.random() * 2 - 1
        this.c2   = Math.random() * 2 - 1
        this.c3   = Math.random() * 2 - 1
        this.bias = Math.random() * 2 - 1
    }

    eval(v1: number, v2: number, v3: number): boolean {
        return this.c1 * v1 + this.c2 * v2 + this.c3 * v3 + this.bias > 0
    }
}

class CarAI {
    private actionsActivations: any = {} // Object containing "action: ActivationParams" associations for all possible actions of the car

    constructor(carSimulation: CarSimulation) {
        // Init actionsActivations
        Object.keys(carSimulation.actions).forEach((key) => {
            this.actionsActivations[key] = new ActivationParams()
        })
    }

    update(carSimulation: CarSimulation) {
        const distances = carSimulation.getDistToObstacles()
        Object.keys(carSimulation.actions).forEach((key) => {
            carSimulation.actions[key] = this.actionsActivations[key].eval(
                distances.front,
                distances.left,
                distances.right,
            )
        })
    }

    fitness(carSimulation: CarSimulation): number {
        // return carSimulation.car.position.x
        return 0;
    }
}

export { CarAI }
export type { DistancesToObstacles }