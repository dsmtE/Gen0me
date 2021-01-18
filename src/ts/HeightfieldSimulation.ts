import * as THREE from 'three';

import Stats from 'three/examples/js/libs/stats.min';
import {OrbitControls} from "three/examples/jsm/controls/OrbitControls";

interface HeightfieldParameters {
    widthExtents: number;
    depthExtents: number;
    width: number;
    depth: number;
    halfWidth: number;
    halfDepth: number;
    maxHeight: number;
    minHeight: number;
};

const heightfieldParameters: HeightfieldParameters = {
    widthExtents: 100,
    depthExtents: 100,
    width: 128,
    depth: 128,
    halfWidth: 128 / 2,
    halfDepth: 128 / 2,
    maxHeight: 8,
    minHeight: - 2,
};

export class HeightfieldSimulation {

    private container: HTMLElement;
    // Global variables
    private scene: THREE.Scene;
    private renderer:THREE.WebGLRenderer;
    private camera: THREE.PerspectiveCamera;

    private stats: Stats;
    
    private clock: THREE.Clock;

    private terrainMesh: THREE.Mesh;

    // Physics variables
    private collisionConfiguration: Ammo.btDefaultCollisionConfiguration;
    private dispatcher: Ammo.btCollisionDispatcher;
    private broadphase: Ammo.btDbvtBroadphase;
    private solver: Ammo.btSequentialImpulseConstraintSolver;
    private physicsWorld: Ammo.btDiscreteDynamicsWorld;
    private dynamicObjects: THREE.Mesh[] = [];
    private transformAux1:  Ammo.btTransform;

    private heightData: Float32Array;
    private ammoHeightData;

    private time: number = 0;
    private objectTimePeriod: number = 3;
    private timeNextSpawn = this.time + this.objectTimePeriod;
    private readonly maxNumObjects = 30;

    constructor(container: HTMLElement) {
        this.container = container;
        
        this.renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        this.container.appendChild(this.renderer.domElement);
        
        this.camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.2, 2000);
        
        this.clock = new THREE.Clock();
        
        //stat
        this.stats = new Stats();
        this.stats.domElement.style.position = 'absolute';
        this.stats.domElement.style.top = '0px';
        container.appendChild(this.stats.domElement);
        
        const controls = new OrbitControls(this.camera, this.renderer.domElement);
        controls.enableZoom = false;
        
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0xbfd1e5);
        
        this.heightData = this.generateHeight(heightfieldParameters.width, heightfieldParameters.depth, heightfieldParameters.minHeight, heightfieldParameters.maxHeight);

        // ---------- initScene ---------- //
        this.camera.position.y = this.heightData[heightfieldParameters.halfWidth + heightfieldParameters.halfDepth * heightfieldParameters.width] * (heightfieldParameters.maxHeight - heightfieldParameters.minHeight) + 5;
        this.camera.position.z = heightfieldParameters.depthExtents / 2;
        this.camera.lookAt(0, 0, 0);

        const geometry = new THREE.PlaneBufferGeometry(heightfieldParameters.widthExtents, heightfieldParameters.depthExtents, heightfieldParameters.width - 1, heightfieldParameters.depth - 1);
        geometry.rotateX(- Math.PI / 2);

        const vertices = geometry.attributes.position.array;

        for (let i = 0, j = 0, l = vertices.length; i < l; i ++, j += 3) {
            // j + 1 because it is the y component that we modify
            vertices[j + 1] = this.heightData[i];
        }

        geometry.computeVertexNormals();

        const groundMaterial = new THREE.MeshPhongMaterial({ color: 0xC7C7C7 });
        this.terrainMesh = new THREE.Mesh(geometry, groundMaterial);
        this.terrainMesh.receiveShadow = true;
        this.terrainMesh.castShadow = true;

        this.scene.add(this.terrainMesh);

        // const textureLoader = new THREE.TextureLoader();
        // textureLoader.load("textures/grid.png", function (texture: THREE.Texture) {
        //     texture.wrapS = THREE.RepeatWrapping;
        //     texture.wrapT = THREE.RepeatWrapping;
        //     texture.repeat.set(heightfieldParameters.width - 1, heightfieldParameters.depth - 1);
        //     groundMaterial.map = texture;
        //     groundMaterial.needsUpdate = true;
        // });

        const light = new THREE.DirectionalLight(0xffffff, 1);
        light.position.set(100, 100, 50);
        light.castShadow = true;
        const dLight = 200;
        const sLight = dLight * 0.25;
        light.shadow.camera.left = - sLight;
        light.shadow.camera.right = sLight;
        light.shadow.camera.top = sLight;
        light.shadow.camera.bottom = - sLight;

        light.shadow.camera.near = dLight / 30;
        light.shadow.camera.far = dLight;

        light.shadow.mapSize.x = 1024 * 2;
        light.shadow.mapSize.y = 1024 * 2;

        this.scene.add(light);

        this.initPhysics();

        window.addEventListener('resize', () => this.onWindowResize(), false);

        this.update();
    }



    private onWindowResize(): void {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }

    private initPhysics(): void {

        // Physics configuration
        this.collisionConfiguration = new Ammo.btDefaultCollisionConfiguration();
        this.dispatcher = new Ammo.btCollisionDispatcher(this.collisionConfiguration);
        this.broadphase = new Ammo.btDbvtBroadphase();
        this.solver = new Ammo.btSequentialImpulseConstraintSolver();
        this.physicsWorld = new Ammo.btDiscreteDynamicsWorld(this.dispatcher, this.broadphase, this.solver, this.collisionConfiguration);
        this.physicsWorld.setGravity(new Ammo.btVector3(0, - 6, 0));

        // Create the terrain body
        const groundShape = this.createTerrainShape();
        const groundTransform = new Ammo.btTransform();
        groundTransform.setIdentity();
        // Shifts the terrain, since bullet re-centers it on its bounding box.
        groundTransform.setOrigin(new Ammo.btVector3(0, (heightfieldParameters.maxHeight + heightfieldParameters.minHeight) / 2, 0));
        const groundMass = 0;
        const groundLocalInertia = new Ammo.btVector3(0, 0, 0);
        const groundMotionState = new Ammo.btDefaultMotionState(groundTransform);
        const groundBody = new Ammo.btRigidBody(new Ammo.btRigidBodyConstructionInfo(groundMass, groundMotionState, groundShape, groundLocalInertia));
        this.physicsWorld.addRigidBody(groundBody);

        this.transformAux1 = new Ammo.btTransform();
    }

    private generateHeight(width: number, depth: number, minHeight: number, maxHeight: number): Float32Array {
        // Generates the height data (a sinus wave)
        const hRange: number = maxHeight - minHeight;
        const w2: number = width / 2;
        const d2: number = depth / 2;
        const phaseMult: number = 12;

        return new Float32Array(width * depth).map( (_, id: number) => {
            const x: number = id % width;
            const y: number = (id/width>>0);

            const radius = Math.sqrt( Math.pow((x - w2) / w2, 2.0) + Math.pow((y - d2) / d2, 2.0));
            const height = (Math.sin(radius * phaseMult) + 1) * 0.5 * hRange + minHeight;

            return height;
        });
    }

    private createTerrainShape(): void {

        // This parameter is not really used, since we are using PHY_FLOAT height data type and hence it is ignored
        const heightScale = 1;

        // Up axis = 0 for X, 1 for Y, 2 for Z. Normally 1 = Y is used.
        const upAxis = 1;

        // hdt, height data type. "PHY_FLOAT" is used. Possible values are "PHY_FLOAT", "PHY_UCHAR", "PHY_SHORT"
        const hdt = "PHY_FLOAT";

        // Set this to your needs (inverts the triangles)
        const flipQuadEdges = false;

        // Creates height data buffer in Ammo heap
        this.ammoHeightData = Ammo._malloc(4 * heightfieldParameters.width * heightfieldParameters.depth);

        // Copy the javascript height data array to the Ammo one.
        let p = 0;
        let p2 = 0;

        for (let j = 0; j < heightfieldParameters.depth; j ++) {
            for (let i = 0; i < heightfieldParameters.width; i ++) {
                // write 32-bit float data to memory
                Ammo.HEAPF32[this.ammoHeightData + p2 >> 2] = this.heightData[p];
                p ++;
                // 4 bytes/float
                p2 += 4;

            }

        }

        // Creates the heightfield physics shape
        const heightFieldShape = new Ammo.btHeightfieldTerrainShape(
            heightfieldParameters.width,
            heightfieldParameters.depth,
            this.ammoHeightData,
            heightScale,
            heightfieldParameters.minHeight,
            heightfieldParameters.maxHeight,
            upAxis,
            hdt,
            flipQuadEdges
    );

        // Set horizontal scale
        const scaleX = heightfieldParameters.widthExtents / (heightfieldParameters.width - 1);
        const scaleZ = heightfieldParameters.depthExtents / (heightfieldParameters.depth - 1);
        heightFieldShape.setLocalScaling(new Ammo.btVector3(scaleX, 1, scaleZ));

        heightFieldShape.setMargin(0.05);

        return heightFieldShape;
    }

    private generateObject(): void {

        const numTypes = 4;
        const objectType = Math.ceil(Math.random() * numTypes);

        let threeObject: THREE.Mesh = null;
        let shape = null;

        const objectSize = 3;
        const margin = 0.05;

        let radius, height;

        switch (objectType) {

            case 1:
                // Sphere
                radius = 1 + Math.random() * objectSize;
                threeObject = new THREE.Mesh(new THREE.SphereBufferGeometry(radius, 20, 20), this.createObjectMaterial());
                shape = new Ammo.btSphereShape(radius);
                shape.setMargin(margin);
                break;
            case 2:
                // Box
                const sx = 1 + Math.random() * objectSize;
                const sy = 1 + Math.random() * objectSize;
                const sz = 1 + Math.random() * objectSize;
                threeObject = new THREE.Mesh(new THREE.BoxBufferGeometry(sx, sy, sz, 1, 1, 1), this.createObjectMaterial());
                shape = new Ammo.btBoxShape(new Ammo.btVector3(sx * 0.5, sy * 0.5, sz * 0.5));
                shape.setMargin(margin);
                break;
            case 3:
                // Cylinder
                radius = 1 + Math.random() * objectSize;
                height = 1 + Math.random() * objectSize;
                threeObject = new THREE.Mesh(new THREE.CylinderBufferGeometry(radius, radius, height, 20, 1), this.createObjectMaterial());
                shape = new Ammo.btCylinderShape(new Ammo.btVector3(radius, height * 0.5, radius));
                shape.setMargin(margin);
                break;
            default:
                // Cone
                radius = 1 + Math.random() * objectSize;
                height = 2 + Math.random() * objectSize;
                threeObject = new THREE.Mesh(new THREE.ConeBufferGeometry(radius, height, 20, 2), this.createObjectMaterial());
                shape = new Ammo.btConeShape(radius, height);
                break;
        }

        threeObject.position.set((Math.random() - 0.5) * heightfieldParameters.width * 0.6, heightfieldParameters.maxHeight + objectSize + 2, (Math.random() - 0.5) * heightfieldParameters.depth * 0.6);

        const mass = objectSize * 5;
        const localInertia = new Ammo.btVector3(0, 0, 0);
        shape.calculateLocalInertia(mass, localInertia);
        const transform = new Ammo.btTransform();
        transform.setIdentity();
        const pos = threeObject.position;
        transform.setOrigin(new Ammo.btVector3(pos.x, pos.y, pos.z));
        const motionState = new Ammo.btDefaultMotionState(transform);
        const rbInfo = new Ammo.btRigidBodyConstructionInfo(mass, motionState, shape, localInertia);
        const body = new Ammo.btRigidBody(rbInfo);

        threeObject.userData.physicsBody = body;

        threeObject.receiveShadow = true;
        threeObject.castShadow = true;

        this.scene.add(threeObject);
        this.dynamicObjects.push(threeObject);

        this.physicsWorld.addRigidBody(body);
    }

    private createObjectMaterial(): THREE.MeshPhongMaterial {
        const c = Math.floor(Math.random() * (1 << 24));
        return new THREE.MeshPhongMaterial({ color: c });
    }

    private updatePhysics(deltaTime: number): void {
        this.physicsWorld.stepSimulation(deltaTime, 10);

        // Update objects
        for (let i = 0, il = this.dynamicObjects.length; i < il; i ++) {

            const objThree = this.dynamicObjects[i];
            const objPhys = objThree.userData.physicsBody;
            const ms = objPhys.getMotionState();
            if (ms) {
                ms.getWorldTransform(this.transformAux1);
                const p = this.transformAux1.getOrigin();
                const q = this.transformAux1.getRotation();
                objThree.position.set(p.x(), p.y(), p.z());
                objThree.quaternion.set(q.x(), q.y(), q.z(), q.w());
            }
        }
    }

    private render(): void {
        const deltaTime = this.clock.getDelta();

        if (this.dynamicObjects.length < this.maxNumObjects && this.time > this.timeNextSpawn) {
            this.generateObject();
            this.timeNextSpawn = this.time + this.objectTimePeriod;
        }

        this.updatePhysics(deltaTime); // run physics
        this.renderer.render(this.scene, this.camera);  // render the scene
        this.time += deltaTime;
    }

    private update(): void {
        this.render();
        this.stats.update();
        requestAnimationFrame( ()=> this.update() );
    }
}

export default Simulation;