import Stats from 'three/examples/js/libs/stats.min.js';
import CarSimulation from './CarSimulation';
import CarAI from './CarAI';

export class Game {
    // private container: HTMLElement;
    private stats: Stats;

    public carSimulation: CarSimulation;
    public carAI: CarAI;
    
    constructor(container: HTMLElement) {
        // this.container = container;

        this.carSimulation = new CarSimulation(container);
        this.carAI = new CarAI(this.carSimulation);


        //stat
        this.stats = new Stats();
        this.stats.domElement.style.position = 'absolute';
        this.stats.domElement.style.top = '0px';
        container.appendChild(this.stats.domElement);
        
        this.update();
    }

    private update(): void {
        this.carSimulation.update();
        const distances = this.carSimulation.getDistToObstacles();
        // this.carAI.update(this.carSimulation);
        this.stats.update();
        requestAnimationFrame(()=> this.update());
    }
}

export default Game;