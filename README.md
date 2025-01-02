# Relatório do Projecto

## Autoria

* Júlia Costa - a22304403

## Link para o Repositório Git

[Link para o Repositório Git](https://github.com/Juhhxx/CG_ToonShader)

## Relatório

### *Toon Shader*

No inicio do projecto, decidi começar por tentar implementar o que achei que seria mais simples, o *toon shader*.

Ja tinha alguma ideia do que tinha de fazer, sabia ao menos que teria de mexer na equação que define como a luz afeta um objecto de forma a torná-la mais "quadrada".

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
        // 0.5 * 4.0 = 2.4
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

**Explicação de como Funciona o Toon Effect no Diffuse Factor**

A técnica que usei para alterar o factor de difusão foi, como descrito acima, a.

![Diffuse Factor Light Curve](https://github.com/Juhhxx/CG_ToonShader/blob/main/Images/desmos-graph%20(6).png)

Estas duas alterações ja criaram um efeito bastante *in-line* com o que eu pretendia, dando o efeito de *toon* que estava á procura.

Colocar imagens do resultado e tambem falar masi sobre o Diffuse e como a função foi alterada (com iamgens tambem).

---

### *Outline Effect*
