<script>
  import {onMount} from 'svelte';
  import AmmoLib from 'three/examples/js/libs/ammo.wasm';
  import Game from './ts/Game';
  
  // Properties
  export let title;
  let game;
  let speed = 0;

  onMount(() => {

    AmmoLib().then(function (ammoLib) {
      Ammo = ammoLib;
      game = new Game(document.getElementById('container'));
    });

    // add interval for svelte reactivity (update property for reactivity in object)
    const interval = setInterval(() => {
      if(game)
        speed = game.carSimulation.speed;
    }, 10);

    return () => { clearInterval(interval); };
  });

  const precisionRound = (x, round) => (Math.round(x*10**round)/10**round).toFixed(round)
  $: speedText = `${precisionRound(speed, 2)} km/h`;
  $:  if (speed >= 60) {
	      console.log(`Wow ! ${speedText}`);
      }

</script>

<style>
  h1 {
    color: lightblue;
  }
</style>
<div class="center">
  <div>
    <h1>{title}</h1>
    {#if game}
      <p>  {speedText} </p>
    {/if}
  </div>
</div>
<div id="container"></div>
