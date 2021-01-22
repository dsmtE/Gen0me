import * as THREE from 'three';

import Stats from 'three/examples/js/libs/stats.min';
import {OrbitControls} from "three/examples/jsm/controls/OrbitControls";
import type { DistancesToObstacles } from './CarAI' 

const carProperties = {
    // Vehicle contants
    chassisWidth: 1.8,
    chassisHeight: .6,
    chassisLength: 4,
    massVehicle: 800,

    wheelAxisPositionBack: -1,
    wheelRadiusBack: .4,
    wheelWidthBack: .3,
    wheelHalfTrackBack: 1,
    wheelAxisHeightBack: .3,

    wheelAxisFrontPosition: 1.7,
    wheelHalfTrackFront: 1,
    wheelAxisHeightFront: .3,
    wheelRadiusFront: .35,
    wheelWidthFront: .2,

    friction: 1000,
    suspensionStiffness: 20.0,
    suspensionDamping: 2.3,
    suspensionCompression: 4.4,
    suspensionRestLength: 0.6,
    rollInfluence: 0.2,

    steeringIncrement: .04,
    steeringClamp: .5,
    maxEngineForce: 2000,
    maxBreakingForce: 100,
};

const DISABLE_DEACTIVATION = 4;

export class CarSimulation {

    private container: HTMLElement;
    private stats: Stats;
    // Global variables
    private scene: THREE.Scene;
    private renderer:THREE.WebGLRenderer;
    private camera: THREE.PerspectiveCamera;
    private clock: THREE.Clock;
    private controls: OrbitControls;

    // Physics variables
    private collisionConfiguration: Ammo.btDefaultCollisionConfiguration;
    private dispatcher: Ammo.btCollisionDispatcher;
    private broadphase: Ammo.btDbvtBroadphase;
    private solver: Ammo.btSequentialImpulseConstraintSolver;
    private physicsWorld: Ammo.btDiscreteDynamicsWorld;
    private transformAux:  Ammo.btTransform;

    private static zeroQuaternion = new THREE.Quaternion(0, 0, 0, 1);

    // private heightData: Float32Array;
    // private ammoHeightData;

    private time: number = 0;
    private objectTimePeriod: number = 3;
    // private timeNextSpawn = this.time + this.objectTimePeriod;
    // private readonly maxNumObjects = 30;

    // Keybord actions
    public actions: { [key:string]:boolean; } = {
        'acceleration': false,
        'braking': false,
        'left': false,
        'right': false,
    };
    private keysActions : { [key:string]:string; } = {
        "KeyW":'acceleration',
        "KeyS":'braking',
        "KeyA":'left',
        "KeyD":'right',
    };

    public getDistToObstacles(): DistancesToObstacles {
        return {
            front: 0.,
            left: 1.,
            right: 0.,
        }
    }
    
    public speed: number = 0.11;

    // scene
    private materialDynamic: THREE.MeshPhongMaterial;
    private materialStatic: THREE.MeshPhongMaterial;
    private materialInteractive: THREE.MeshPhongMaterial;

    private syncList: {(deltaTime: number): void}[];
    
    constructor(container: HTMLElement) {
        this.container = container;
        
        this.renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        this.container.appendChild(this.renderer.domElement);
        
        this.camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.2, 2000);
        
        this.clock = new THREE.Clock();
        
        this.syncList = [];
        
        //stat
        this.stats = new Stats();
        this.stats.domElement.style.position = 'absolute';
        this.stats.domElement.style.top = '0px';
        container.appendChild(this.stats.domElement);
        
        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        // this.controls.enableZoom = false;
        
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0xbfd1e5);
        
        // ---------- initScene ---------- //
        this.camera.position.set(-4.84, 4.39, -35.11);
        this.camera.lookAt(0.33, -0.40, 0.85);
        
        var ambientLight = new THREE.AmbientLight(0x404040);
        this.scene.add(ambientLight);

        var dirLight = new THREE.DirectionalLight(0xffffff, 1);
        dirLight.position.set(10, 10, 5);
        this.scene.add(dirLight);


        // this.terrainMesh.receiveShadow = true;
        // this.terrainMesh.castShadow = true;

        this.materialDynamic = new THREE.MeshPhongMaterial({ color:0xfca400 });
        this.materialStatic = new THREE.MeshPhongMaterial({ color:0x999999 });
        this.materialInteractive = new THREE.MeshPhongMaterial({ color:0x990000 });
                
        this.initPhysics();
        this.createObjects();
        this.registerListener();
    }

    private registerListener() : void {
        window.addEventListener('resize', () => this.onWindowResize(), false);
        window.addEventListener('keydown', (e) => this.keydown(e));
        window.addEventListener('keyup', (e) => this.keyup(e));
    }

    private onWindowResize(): void {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }

    private keyup(e: KeyboardEvent): boolean {
        if(this.keysActions[e.code]) {
            this.actions[this.keysActions[e.code]] = false;
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
        return false;
    }
    private keydown(e: KeyboardEvent): boolean {
        if(this.keysActions[e.code]) {
            this.actions[this.keysActions[e.code]] = true;
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
        return false;
    }

    private initPhysics(): void {

        // Physics configuration
        this.collisionConfiguration = new Ammo.btDefaultCollisionConfiguration();
        this.dispatcher = new Ammo.btCollisionDispatcher(this.collisionConfiguration);
        this.broadphase = new Ammo.btDbvtBroadphase();
        this.solver = new Ammo.btSequentialImpulseConstraintSolver();
        this.physicsWorld = new Ammo.btDiscreteDynamicsWorld(this.dispatcher, this.broadphase, this.solver, this.collisionConfiguration);
        this.physicsWorld.setGravity(new Ammo.btVector3(0, - 9.82, 0));
        this.transformAux = new Ammo.btTransform();
    }

    private createWheelMesh(radius: number, width: number): THREE.Mesh {
        const t = new THREE.CylinderGeometry(radius, radius, width, 24, 1);
        t.rotateZ(Math.PI / 2);
        const mesh = new THREE.Mesh(t, this.materialInteractive);
        mesh.add(new THREE.Mesh(new THREE.BoxGeometry(width * 1.5, radius * 1.75, radius*.25, 1, 1, 1), this.materialInteractive));
        return mesh;
    }

    private createChassisMesh(w: number, l: number, h: number): THREE.Mesh {
        var shape = new THREE.BoxGeometry(w, l, h, 1, 1, 1);
        return new THREE.Mesh(shape, this.materialInteractive);
    }

    private createBox(pos: THREE.Vector3, quat: THREE.Quaternion, w: number, l: number, h: number, mass: number = 0, friction: number = 1) {
        var material = mass > 0 ? this.materialDynamic : this.materialStatic;
        var shape = new THREE.BoxGeometry(w, l, h, 1, 1, 1);
        var geometry = new Ammo.btBoxShape(new Ammo.btVector3(w * 0.5, l * 0.5, h * 0.5));

        const mesh = new THREE.Mesh(shape, material);
        mesh.position.copy(pos);
        mesh.quaternion.copy(quat);
        this.scene.add(mesh);

        const transform = new Ammo.btTransform();
        transform.setIdentity();
        transform.setOrigin(new Ammo.btVector3(pos.x, pos.y, pos.z));
        transform.setRotation(new Ammo.btQuaternion(quat.x, quat.y, quat.z, quat.w));
        var motionState = new Ammo.btDefaultMotionState(transform);

        var localInertia = new Ammo.btVector3(0, 0, 0);
        geometry.calculateLocalInertia(mass, localInertia);

        var rbInfo = new Ammo.btRigidBodyConstructionInfo(mass, motionState, geometry, localInertia);
        var body = new Ammo.btRigidBody(rbInfo);

        body.setFriction(friction);
        //body.setRestitution(.9);
        //body.setDamping(0.2, 0.2);

        this.physicsWorld.addRigidBody(body);

        if (mass > 0) {
            body.setActivationState(DISABLE_DEACTIVATION);
             
            // Sync physics and graphics
            this.syncList.push((deltaTime: number) => {
                const ms = body.getMotionState();
                if (ms) {
                    ms.getWorldTransform(this.transformAux);
                    var p = this.transformAux.getOrigin();
                    var q = this.transformAux.getRotation();
                    mesh.position.set(p.x(), p.y(), p.z());
                    mesh.quaternion.set(q.x(), q.y(), q.z(), q.w());
                }
            });
        }
    }

    private createVehicle(pos: THREE.Vector3, quat: THREE.Quaternion) {
        // Chassis
        var geometry = new Ammo.btBoxShape(new Ammo.btVector3(carProperties.chassisWidth * .5, carProperties.chassisHeight * .5, carProperties.chassisLength * .5));
        var transform = new Ammo.btTransform();
        transform.setIdentity();
        transform.setOrigin(new Ammo.btVector3(pos.x, pos.y, pos.z));
        transform.setRotation(new Ammo.btQuaternion(quat.x, quat.y, quat.z, quat.w));
        var motionState = new Ammo.btDefaultMotionState(transform);
        var localInertia = new Ammo.btVector3(0, 0, 0);
        geometry.calculateLocalInertia(carProperties.massVehicle, localInertia);
        var body = new Ammo.btRigidBody(new Ammo.btRigidBodyConstructionInfo(carProperties.massVehicle, motionState, geometry, localInertia));
        body.setActivationState(DISABLE_DEACTIVATION);
        this.physicsWorld.addRigidBody(body);
        const chassisMesh = this.createChassisMesh(carProperties.chassisWidth, carProperties.chassisHeight, carProperties.chassisLength);
        this.scene.add(chassisMesh);

        // Raycast Vehicle
        var engineForce = 0;
        var vehicleSteering = 0;
        var breakingForce = 0;
        var tuning = new Ammo.btVehicleTuning();
        var rayCaster = new Ammo.btDefaultVehicleRaycaster(this.physicsWorld);
        var vehicle = new Ammo.btRaycastVehicle(tuning, body, rayCaster);
        vehicle.setCoordinateSystem(0, 1, 2);
        this.physicsWorld.addAction(vehicle);

        // Wheels
        var FRONT_LEFT = 0;
        var FRONT_RIGHT = 1;
        var BACK_LEFT = 2;
        var BACK_RIGHT = 3;
        var wheelMeshes: THREE.Mesh[] = [];
        var wheelDirectionCS0 = new Ammo.btVector3(0, -1, 0);
        var wheelAxleCS = new Ammo.btVector3(-1, 0, 0);

        const addWheel = (isFront: boolean, pos: Ammo.btVector3, radius: number, width: number, index: number) => {

            const wheelInfo = vehicle.addWheel(pos, wheelDirectionCS0, wheelAxleCS, carProperties.suspensionRestLength, radius, tuning, isFront);

            wheelInfo.set_m_suspensionStiffness(carProperties.suspensionStiffness);
            wheelInfo.set_m_wheelsDampingRelaxation(carProperties.suspensionDamping);
            wheelInfo.set_m_wheelsDampingCompression(carProperties.suspensionCompression);
            wheelInfo.set_m_frictionSlip(carProperties.friction);
            wheelInfo.set_m_rollInfluence(carProperties.rollInfluence);

            wheelMeshes[index] = this.createWheelMesh(radius, width);
            this.scene.add(wheelMeshes[index]);
        }

        addWheel(true, new Ammo.btVector3(carProperties.wheelHalfTrackFront, carProperties.wheelAxisHeightFront, carProperties.wheelAxisFrontPosition), carProperties.wheelRadiusFront, carProperties.wheelWidthFront, FRONT_LEFT);
        addWheel(true, new Ammo.btVector3(-carProperties.wheelHalfTrackFront, carProperties.wheelAxisHeightFront, carProperties.wheelAxisFrontPosition), carProperties.wheelRadiusFront, carProperties.wheelWidthFront, FRONT_RIGHT);
        addWheel(false, new Ammo.btVector3(-carProperties.wheelHalfTrackBack, carProperties.wheelAxisHeightBack, carProperties.wheelAxisPositionBack), carProperties.wheelRadiusBack, carProperties.wheelWidthBack, BACK_LEFT);
        addWheel(false, new Ammo.btVector3(carProperties.wheelHalfTrackBack, carProperties.wheelAxisHeightBack, carProperties.wheelAxisPositionBack), carProperties.wheelRadiusBack, carProperties.wheelWidthBack, BACK_RIGHT);

        
        // Sync keybord actions and physics and graphics
        this.syncList.push((deltaTime : number) => {
            this.speed = vehicle.getCurrentSpeedKmHour();
            
            breakingForce = 0;
            engineForce = 0;

            if (this.actions.acceleration) {
                if (this.speed < -1)
                    breakingForce = carProperties.maxBreakingForce;
                else engineForce = carProperties.maxEngineForce;
            }
            if (this.actions.braking) {
                if (this.speed > 1)
                    breakingForce = carProperties.maxBreakingForce;
                else engineForce = -carProperties.maxEngineForce / 2;
            }
            if (this.actions.left) {
                if (vehicleSteering < carProperties.steeringClamp)
                    vehicleSteering += carProperties.steeringIncrement;
            }
            else {
                if (this.actions.right) {
                    if (vehicleSteering > -carProperties.steeringClamp)
                        vehicleSteering -= carProperties.steeringIncrement;
                }
                else {
                    if (vehicleSteering < -carProperties.steeringIncrement)
                        vehicleSteering += carProperties.steeringIncrement;
                    else {
                        if (vehicleSteering > carProperties.steeringIncrement)
                            vehicleSteering -= carProperties.steeringIncrement;
                        else {
                            vehicleSteering = 0;
                        }
                    }
                }
            }

            vehicle.applyEngineForce(engineForce, BACK_LEFT);
            vehicle.applyEngineForce(engineForce, BACK_RIGHT);
    
            vehicle.setBrake(breakingForce / 2, FRONT_LEFT);
            vehicle.setBrake(breakingForce / 2, FRONT_RIGHT);
            vehicle.setBrake(breakingForce, BACK_LEFT);
            vehicle.setBrake(breakingForce, BACK_RIGHT);
    
            vehicle.setSteeringValue(vehicleSteering, FRONT_LEFT);
            vehicle.setSteeringValue(vehicleSteering, FRONT_RIGHT);
    
            var tm, p, q, i;
            var n = vehicle.getNumWheels();
            for (i = 0; i < n; i++) {
                vehicle.updateWheelTransform(i, true);
                tm = vehicle.getWheelTransformWS(i);
                p = tm.getOrigin();
                q = tm.getRotation();
                wheelMeshes[i].position.set(p.x(), p.y(), p.z());
                wheelMeshes[i].quaternion.set(q.x(), q.y(), q.z(), q.w());
            }
    
            tm = vehicle.getChassisWorldTransform();
            p = tm.getOrigin();
            q = tm.getRotation();
            chassisMesh.position.set(p.x(), p.y(), p.z());
            chassisMesh.quaternion.set(q.x(), q.y(), q.z(), q.w());
        });
    }

    private createObjects() : void {
        this.createBox(new THREE.Vector3(0, -0.5, 0), CarSimulation.zeroQuaternion, 75, 1, 75, 0, 2);

        var quaternion = new THREE.Quaternion(0, 0, 0, 1);
        quaternion.setFromAxisAngle(new THREE.Vector3(1, 0, 0), -Math.PI / 18);
        this.createBox(new THREE.Vector3(0, -1.5, 0), quaternion, 8, 4, 10, 0);

        var size = .75;
        var nw = 8;
        var nh = 6;
        for (var j = 0; j < nw; j++)
            for (var i = 0; i < nh; i++)
                this.createBox(new THREE.Vector3(size * j - (size * (nw - 1)) / 2, size * i, 10), CarSimulation.zeroQuaternion, size, size, size, 10);

        this.createVehicle(new THREE.Vector3(0, 4, -20), CarSimulation.zeroQuaternion);
    }

    private updatePhysics(deltaTime: number): void {
        this.syncList.forEach(x => x(deltaTime));
        this.physicsWorld.stepSimulation(deltaTime, 10);
    }

    private render(): void {
        this.renderer.render(this.scene, this.camera);  // render the scene
    }

    public update(): void {
        const deltaTime = this.clock.getDelta();
        this.updatePhysics(deltaTime); // run physics
        this.controls.update();
        this.render();
        this.stats.update();
        this.time += deltaTime;
    }
}

export default CarSimulation;