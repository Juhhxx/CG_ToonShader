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

---

### *Outline Effect*
