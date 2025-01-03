# Relatório do Projecto

## Autoria

* Júlia Costa - a22304403

## Link para o Repositório Git

[Link para o Repositório Git](https://github.com/Juhhxx/CG_ToonShader)

## Relatório

### ***Toon Shader***

No inicio do projecto, decidi começar por tentar implementar o que achei que seria mais simples, o *toon shader*.

Ja tinha alguma ideia do que fazer, sabia ao menos que teria de mexer na equação que define como a luz afeta um objecto de forma a torná-la mais "quadrada".

### Pesquisa

Pesquisando um pouco pela internet sobre *toon shaders* consegui encontrar alguns videos que me ajudaram a perceber melhor os conceitos base para fazer o meu shader.

Lista com alguns dos recursos online que encontrei:

* [Toon Shading & Rim Lighting // OpenGL Tutorial #34 - OGLDEV](https://www.youtube.com/watch?v=h15kTY3aWaY);
* [OpenGL 3D Game Tutorial 30: Cel Shading - ThinMatrix](https://www.youtube.com/watch?v=dzItGHyteng);
* [Toon Shading Fundamentals - Panthavma's Blog](https://panthavma.com/articles/shading/toonshading/);
* [Toon Shading - Lighthouse3d](http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/toon-shading-version-i/).

### Implementação

No inicio, de forma a testar rapidamente as técnicas que ia retirando dos recursos que encontrei, usei uma cópia to shader do professor `phong_pp`, fazendo as minhas alterações no *fragment shader*.

>**Nota:** Este processo para testar conceitos ainda demorou um bocado a avançar, ao princípio estava um pouco confusa onde devia mexer, revi bastantes vezes os resources que encontrei e os slides das aulas do professor para poder começar a editar com confiança as coisas que precisava.

Através dos resources que encontrei percebi que, apartir do shader ja implementado do professor, iria precisar de mexer no fator de *difusão* da luz de modo a que ele deixa-se de ter uma curva *smooth* e ao inves disso fosse interpolando entre um numero de valores definidos.

Comecei então por editar o ficheiro `phong_pp.frag` com as seguintes alterações:

* Adicionei duas constantes que definem quantos niveis de luminusidade o shader permite:

    ```glsl
    // Define quantos niveis de luminusidade o shader vai permitir
    const int ToonColorLevels = 4;

    // Define o factor de escala a ser aplicado depois de se multiplicar pelo ToonColorLevels
    const float ToonScaleFactor = 1.0 / ToonColorLevels;
    ```

* Editei o método que calcula as *points lights* de modo a mudar a difusão de uma *smooth curve* para uma *stair function*:

    ```glsl
    vec3 ComputePoint(Light light, vec3 worldPos, vec3 worldNormal, vec4 materialColor)
    {
        vec3  lightDir = normalize(worldPos - light.position);
        float d = clamp(-dot(worldNormal, lightDir), 0, 1);

        // Multiplicando o valor de d (varia de 0 a 1) pelo numero de niveis n que
        // queremos e dando um floor (arredondar para baixo), estamos a constrangir
        // o mesmo a apenas valores entre 0 e n-1, multiplicando depois pelo fator
        // de escala (1.0 / n) de modo a voltar ao intervalo entre 0 e 1.
        //
        // Exemplo com n = 4.0 e d = 0.6:
        // 
        // 0.6 * 4.0 = 2.4
        // floor(2.4) = 2.0
        // 2.0 * (1.0 / 4.0) = 0.5

        d = floor(d * ToonColorLevels) * ToonScaleFactor;

        vec3  v = normalize(ViewPos - worldPos);
        // Light dir is from light to point, but we want the other way around, hence the V - L
        vec3  h =  normalize(v - lightDir);
        float s = MaterialSpecular.x * pow(max(dot(h, worldNormal), 0), MaterialSpecular.y);

        return clamp(d * materialColor.xyz + s, 0, 1) * light.color.rgb * light.intensity * ComputeAttenuation(light, worldPos);
    }
    ```

#### Explicação de como Funciona o Toon Effect no Diffuse Factor

A técnica que usei para alterar o factor de difusão foi, como descrito acima, a aplicação de um floor á multiplicação do resultado original do Diffuse com o número de *shades* (neste caso 4) se sombra que pretendo aplicar, isto vai tornar os resultados da difusão, que variam entre 0 e 1, em resutlados que variam entre, neste caso, 0 e 3, este resultado é depois dividido pelo numero de *shades* para ser reduzido novamente á escala 0 a 1.

Isto vai fazer com que o range de *shades* fique limitado a esses 4 valores entre 0 e 3, dando assim o efeito de *hard transitions* entre os tons de sombra que é a base dos *toon shaders*.

Abaixo apresento um gráfico que mostra a resultado normal do cálculo da difusão, representada a vermelho (🟥), juntamente com o resultado da mesma depois de lhe ser aplicado o efeito de *toon*, representado a verde (🟩).

![Diffuse Factor Light Curve](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/desmos-graph%20(6).png)

>🟥: *d( x )* = cos( *b* )
>
>*b* = *x* \* π / 180
>
>🟩: *t( x )* = floor( *d( x )* \* 4 )

Estas duas alterações ja criaram um efeito bastante *in-line* com o que eu pretendia, dando o efeito de *toon* que estava á procura.

Aqui está o resultado antes:

![Shader Test Before](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/test_before.png)

Depois:

![Shader Test After](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/test_after.png)

#### Problemas Encontrados

#### Luz Especular

Depois de já ter a parte do Diffuse Factor a funcionar e de mexer um bocado na cena, verifiquei, tanto por observar a cena como por rever os resources que tinha juntado, que ainda faltava mexer no **Specular Factor** que estava a produzir ainda um brilho muito *"realístico"*.

![Specular Light Realistic](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/specular_problem.png)

Para resolver este problema pensei em duas soluções possiveis, a primeira seria aplicar o efeito de *toon* também à luz especular, a segunda era desligar completamente a luz especular, visto que a mesma tenta reproduzir um efeito mais realístico que normalmente não é o desejado num sahder deste tipo.

Decidi então exprimentar as duas maneiras.

#### Luz Especular com Efeito de *Toon*

![Splecular Light Toon 1](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/specular_fix1.png)

No geral, gostei de como ficou, mas como pode ser observado, como estou a usar um factor (`ToonColorLevels`) de 4 no efeito de *toon*, a luz especular acaba por ainda ter alguma variação.

O resultado desta tentativa não me agradou, então acabei por exprimentar reduzir o factor de *toon* para 2 apenas na luz especular e ver como ficava o resultado.

#### Luz Especular com Efeito de *Toon* Reduzido

![Specular Light Toon 2](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/specular_fixfinal.png)

Este resultado já me agradou mais e foi o que acabei por seguir com.

#### Luz Especular Desligada

![Specular Light Off](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/specular_fix2.png)

Sem a luz especular, a cena ganhava, definitivamente, um ar mais *cartoony*, no entanto não gostei tanto do efeito pois assim não existia diferença entre os objectos mais metálicos e os mais plásticos.

Pelo motivo acima referido, e como gostei bastante do resultado final da luz especular com o efeito *toon*, acabei por decidir ficar, como já tinha referido, com o efeito *toon* na luz especular com o factor reduzido.

#### Luz Ambiente

Outro problema que encontrei enquanto esplorava a cena foi que, na parte mais escura das sombras, algum tipo de iluminação que nâo a directa estava a intreferir com *output* do shader e a causar gradientes suaves que não encaixavam a a estética *toon*.

#### Efeito da Luz Ambiente nos Objectos

![Ambient Light Cubemap Influence](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/ambientlight_problem.png)

Sendo que estes objectos não eram *Emissive* nem existiam outras luzes em cena, deduzi que o problema devia estar na componente Ambiente da equação da luz.

Quando fui ver como estava a ser calculada a luz ambiente, encontrei isto:

```glsl
// Ambient component - get data from 4th mipmap of the cubemap (effective hardware blur)
vec3 envLighting = EnvColor.xyz * MaterialColor.xyz * textureLod(EnvTextureCubeMap, worldNormal, 8).xyz;
```

Ao ver o pedaço de código, que já haviamos falado em aula, percebi logo que só havia duas possibilidadeds do que podia estar a causar o efeito, a **Cor do Ambiente** (`EnvColor.xyz`) que sendo ela variante entre *Top*, *Mid* e *Bottom* poderia estar a afetar o resultado final como visto acima, ou a contribuição da ***Skybox*** (`textureLod(EnvTextureCubeMap, worldNormal, 8).xyz`) que sendo uma textura com várias cores poderia estar a a causar o efeito.

Comecei por então explorar a teoria da Cor Ambiente, indo mexer nos seus valores na esperança de encontrar uma solução, fui então ao *setup* da cena vero nde estes valores estavam a ser definidos.

Encontrei o seguinte método do professor que definia os valroes ambientes na cena:

```c#
static void SetupEnvironment()
{
    var cubeMap = new Texture();
    cubeMap.LoadCube("Textures/cube_*.jpg");

    var env = OpenTKApp.APP.mainScene.environment;

    env.Set("Color", new Color4(0.2f, 0.2f, 0.2f, 1.0f));
    env.Set("ColorTop", new Color4(0.0f, 1.0f, 1.0f, 1.0f));
    env.Set("ColorMid", new Color4(1.0f, 1.0f, 1.0f, 1.0f));
    env.Set("ColorBottom", new Color4(0.0f, 0.25f, 0.0f, 1.0f));
    env.Set("FogDensity", 0.000001f);
    env.Set("FogColor", Color.DarkCyan);
    env.Set("CubeMap", cubeMap);
}
```

Ao pricípio reparei que os valores *Top*, *Mid* e *Bottom* da Cor Ambiente eram bastante próximos, isto pos alguams dúvidas à minha teoria, mas decidi exprimentar à mesma para ver os resultados.

Pondo os 3 valores todos iguais, notei que nada mudou.

Isto desprovou a teoria de que estes valores estavam a causar o efeito, e depois de pensar melhor e rever algumas coisas, percebi o porquê, o shader que estava a usar não tomava sequer em consideração as cores *Top*, *Mid* e *Bottom*, apenas a cor Ambiente comum definida em `env.Set("Color", new Color4(0.2f, 0.2f, 0.2f, 1.0f));`.

Isto levou-me a testar a minha segunda teoria, que era a que eu suspeitava mais, a da contribuição da *Skybox*.

Esta estava a contribuir com uma textura borrada do seu *cube map* original, multiplicando o resultado final da cor por um pixel correspondente. Como o *cube map* tinha uma textura detalhada com muitas variações de cor, ja tinha na cebeça desde o princípio que este seria mais provavelmente o culpado.

Decidi então remover a componente do *cube map* do cálculo da luz ambiente, ficando com o seguinte código:

```glsl
vec3 envLighting = EnvColor.xyz * MaterialColor.xyz;
```

O código acima apenas usa as contribuições da Cor Ambiente (`EnvColor.xyz`) e da Cor do Material (`MaterialColor.xyz`), deixando de parte a contribuição da *Skybox* pelas razões acima referidas.

Ao correr o projecto deparei-me com isto:

#### Efeito da Luz Ambiente com a Contribuição da *Skybox* Desligada

![Ambient Light Cubemap Off](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/ambientlight_fix.png)

Esta mudança, como pode ser observado acima, resolveu o problema, chegando a um resultado muito mais *cartoony* do que estava antes, tendo deixado até as partes sem luz dos objectos menos escuras que antes estavam a ser afetadas pela cor da *Skybox*.

### Efeito Final de *Toon*

Para dar os toques finais ao shader, organizei-o melhor, criando um método para tratar de aplicar o efeito de *toon* aos valores desejados e removi membros que estavam a ser inutilizados pelo shader para limpar espaço.

O método do cálculo do efeito de *toon* ficou o seguinte:

```glsl
const int ToonColorLevels = 4;
const int ToonColorLevelsSpec = 2;

void AddToonEffect(inout float value, int toonFactor)
{
    // Restrain the given value into the number of factors given by toonFactor
    value = floor(value * toonFactor) / toonFactor;
}
```

Acabei por definir duas constantes juntamente com o método, uma para o *Difuse Factor* (`ToonColorLevels`) e outra para o *Specular Factor* (`ToonColorLevelsSpec`).

Depois apliquei este método tambèm à luz *Spot* e *Directional*.

Os efeitos finais foram os seguintes:

#### *Toon Shader* com Luz Direcional

![Toon Shader Final Directional](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/toon_final_directional.png)

#### *Toon Shader* com Luz Pontual

![Toon Shader Final Point](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/toon_final_point.png)

#### *Toon Shader* com Luz em Foco

![Toon Shader Final Spot](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/toon_final_Spot.png)

No final acabei com o efeito acima mostrado, gostei de como ficou o resultado final e decidi que já estava bastante bom para o que eu queria.

---

### ***Outline Effect***

Depois de ter o *toon shader* completo, faltava-me a parte, que ccnsiderei, mais difícil, as linhas de *outline*.

Tinha alguma ideia do que talvez precisaria de fazer, mas admito que no princípio não fazia ideia por onde começar.

### Pesquisa

Pesquisando pela internet sobre como fazer linhas de *outline* em objectos consegui encontrar vários recursos que me ajudaram a perceber melhor o que teria de fazer para atingir o efeito que queria.

Lista com alguns dos recursos online que encontrei:

* [GL3 Shaders | Part 07 - Per-Object Outline Effect - Amazing Max Stuff](https://www.youtube.com/watch?v=SahxZjPWsQw);
* [OpenGL Tutorial 15 - Stencil Buffer & Outlining - Victor Gordan](https://www.youtube.com/watch?v=ngF9LWWxhd0);
* [5 ways to draw an outline - Alexander Ameye](https://ameye.dev/notes/rendering-outlines/);
* [Answer to : outline object effect - Martin Sojka](https://gamedev.stackexchange.com/questions/34652/outline-object-effect);
* [Building the classic outline shader](https://www.videopoetics.com/tutorials/pixel-perfect-outline-shaders-unity/#building-the-classic-outline-shader).

### Implementação