namespace Library81.Client.wwwroot.js
{
    public class book3d
    {// wwwroot/js/book3d.js
// Livre 3D Babylon.js – procédural + animations
(function () {
        "use strict";

        const ease = () => {
            const e = new BABYLON.CubicEase();
            e.setEasingMode(BABYLON.EasingFunction.EASINGMODE_EASEINOUT);
            return e;
        };

        function deg(rad) {
            return rad * 180 / Math.PI;
        }

        // Petite fabrique d'animations
        function animateRotationY(mesh, from, to, fps = 60, durationMs = 700) {
            const frames = Math.round((durationMs / 1000) * fps);
            const anim = new BABYLON.Animation(
                `rotY_${Date.now()}`,
                "rotation.y",
                fps,
                BABYLON.Animation.ANIMATIONTYPE_FLOAT,
                BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT
            );
            anim.setKeys([
                { frame: 0, value: from },
                { frame: frames, value: to }
            ]);
            anim.setEasingFunction(ease());
            mesh.animations = mesh.animations || [];
            mesh.animations.push(anim);
            return anim;
        }

        function animatePosition(mesh, from, to, fps = 60, durationMs = 700) {
            const frames = Math.round((durationMs / 1000) * fps);
            const anim = new BABYLON.Animation(
                `pos_${Date.now()}`,
                "position",
                fps,
                BABYLON.Animation.ANIMATIONTYPE_VECTOR3,
                BABYLON.Animation.ANIMATIONLOOPMODE_CONSTANT
            );
            anim.setKeys([
                { frame: 0, value: from },
                { frame: frames, value: to }
            ]);
            anim.setEasingFunction(ease());
            mesh.animations = mesh.animations || [];
            mesh.animations.push(anim);
            return anim;
        }

        // Création du livre
        function createBook(scene, options = {}) {
            const {
                width = 0.21,  // 21 cm
                height = 0.297, // 29.7 cm
                thickness = 0.03, // 3 cm total
                pageCount = 24,   // nombre de pages animables (recto-verso = 12 feuilles)
                coverThickness = 0.004,
                pageThickness = 0.0006,
                pageTint = new BABYLON.Color3(0.98, 0.97, 0.94),
                coverColor = new BABYLON.Color3(0.24, 0.16, 0.12),
                spineWidth = 0.016
            } = options;

            const book = new BABYLON.TransformNode("book", scene);

            // Matériaux
            const matCover = new BABYLON.PBRMaterial("matCover", scene);
            matCover.roughness = 0.6;
            matCover.metallic = 0.0;
            matCover.albedoColor = coverColor;

            const matPage = new BABYLON.PBRMaterial("matPage", scene);
            matPage.albedoColor = pageTint;
            matPage.roughness = 0.9;
            matPage.metallic = 0.0;

            const matSpine = new BABYLON.PBRMaterial("matSpine", scene);
            matSpine.albedoColor = coverColor.scale(0.9);
            matSpine.roughness = 0.55;
            matSpine.metallic = 0.0;

            // DOS
            const spine = BABYLON.MeshBuilder.CreateBox("spine", {
                width: spineWidth,
                height: height,
                depth: thickness
            }, scene);
            spine.material = matSpine;
            spine.parent = book;
            spine.position.x = -width / 2 + spineWidth / 2;

            // COUVERTURE GAUCHE (fixe)
            const coverL = BABYLON.MeshBuilder.CreateBox("coverL", {
                width: (width - spineWidth) / 2,
                height: height,
                depth: coverThickness
            }, scene);
            coverL.material = matCover;
            coverL.parent = book;
            coverL.position.x = -spineWidth / 2 - coverL.getBoundingInfo().boundingBox.extendSizeWorld.x;
            coverL.position.z = -thickness / 2 + coverThickness / 2;

            // PIVOT pour COUVERTURE DROITE (animée)
            const coverR_pivot = new BABYLON.TransformNode("coverR_pivot", scene);
            coverR_pivot.parent = book;
            coverR_pivot.position = new BABYLON.Vector3(-spineWidth / 2, 0, -thickness / 2 + coverThickness / 2);

            const coverR = BABYLON.MeshBuilder.CreateBox("coverR", {
                width: (width - spineWidth) / 2,
                height: height,
                depth: coverThickness
            }, scene);
            coverR.material = matCover;
            coverR.parent = coverR_pivot;
            coverR.position.x = coverR.getBoundingInfo().boundingBox.extendSizeWorld.x;

            // BLOC PAGES (empilage)
            const pagesRoot = new BABYLON.TransformNode("pagesRoot", scene);
            pagesRoot.parent = book;
            pagesRoot.position.z = -thickness / 2 + coverThickness + (pageThickness * pageCount) / 2;

            const pagesRightPivot = []; // pivots des pages qui se tournent vers la gauche

            for (let i = 0; i < pageCount; i++) {
                const pagePivot = new BABYLON.TransformNode(`pagePivot_${i}`, scene);
                pagePivot.parent = pagesRoot;

                // Position Z dans l'épaisseur
                const z = (i * pageThickness) - (pageCount * pageThickness) / 2;
                pagePivot.position = new BABYLON.Vector3(0, 0, z);

                // Page (moitié droite qui se tourne individuellement autour du dos)
                const page = BABYLON.MeshBuilder.CreateBox(`page_${i}`, {
                    width: (width - spineWidth),   // double page à plat
                    height: height,
                    depth: pageThickness
                }, scene);
                page.material = matPage;
                page.parent = pagePivot;

                page.position.x = (width - spineWidth) / 2 - 0.00001; // léger epsilon pour éviter Z-fighting

                // Inclinaison subtile pour un aspect réaliste :
                const belly = 0.003 * Math.sin((i / pageCount) * Math.PI);
                pagePivot.rotation.z = belly;

                pagesRightPivot.push(pagePivot);
            }

            // Réglage caméra/lumières
            const camera = new BABYLON.ArcRotateCamera(
                "cam",
                BABYLON.Tools.ToRadians(-35),
                BABYLON.Tools.ToRadians(65),
                0.8,
                BABYLON.Vector3.Zero(),
                scene
            );

            // Uniquement attacher les contrôles si le canvas existe
            const canvas = scene.getEngine().getRenderingCanvas();
            if (canvas) {
                camera.attachControl(canvas, true);
            }

            // Lumières
            const hemi = new BABYLON.HemisphericLight("hemi", new BABYLON.Vector3(0, 1, 0), scene);
            hemi.intensity = 0.9;
            const dir = new BABYLON.DirectionalLight("dir", new BABYLON.Vector3(-0.3, -1, 0.3), scene);
            dir.position = new BABYLON.Vector3(0.6, 0.8, -0.6);
            dir.intensity = 0.6;

            // Ombres douces
            const shadowGen = new BABYLON.ShadowGenerator(1024, dir);
            shadowGen.useExponentialShadowMap = true;
            [spine, coverL, coverR].forEach(m => shadowGen.addShadowCaster(m));

            scene.environmentIntensity = 0.7;

            // Créer l'environnement par défaut de manière sécurisée
            try {
                scene.createDefaultEnvironment({
                    createSkybox: false,
                    createGround: true,
                    groundSize: 2.0
                });
            } catch (error) {
                console.warn("Impossible de créer l'environnement par défaut:", error);
            }

            // État du livre
            let isOpen = false;
            let currentPage = 0; // combien de pages ont été tournées (0..pageCount)

            // API : Ouvrir / Fermer (couvre droite)
            const open = (duration = 700) => {
                if (isOpen) return scene;
                coverR_pivot.animations = [];
                const start = coverR_pivot.rotation.y;
                const end = -Math.PI; // ouvrir vers la gauche
                const anim = animateRotationY(coverR_pivot, start, end, 60, duration);
                scene.beginAnimation(coverR_pivot, 0, 60, false);
                isOpen = true;
                return scene;
            };

            const close = (duration = 700) => {
                if (!isOpen) return scene;
                // Ramener toutes les pages au repos:
                for (let i = currentPage - 1; i >= 0; i--) {
                    const p = pagesRightPivot[i];
                    if (p) {
                        p.animations = [];
                        const anim = animateRotationY(p, p.rotation.y, 0, 60, duration * 0.6);
                        scene.beginAnimation(p, 0, 60, false);
                    }
                }
                currentPage = 0;

                coverR_pivot.animations = [];
                const start = coverR_pivot.rotation.y;
                const end = 0; // refermer
                const anim = animateRotationY(coverR_pivot, start, end, 60, duration);
                scene.beginAnimation(coverR_pivot, 0, 60, false);
                isOpen = false;
                return scene;
            };

            // API : Page suivante / précédente
            const nextPage = (duration = 500) => {
                if (!isOpen || currentPage >= pageCount) return false;
                const p = pagesRightPivot[currentPage];
                if (!p) return false;
                p.animations = [];
                const anim = animateRotationY(p, p.rotation.y, -Math.PI, 60, duration);
                scene.beginAnimation(p, 0, 60, false);
                currentPage++;
                return true;
            };

            const prevPage = (duration = 500) => {
                if (!isOpen || currentPage <= 0) return false;
                const i = currentPage - 1;
                const p = pagesRightPivot[i];
                if (!p) return false;
                p.animations = [];
                const anim = animateRotationY(p, p.rotation.y, 0, 60, duration);
                scene.beginAnimation(p, 0, 60, false);
                currentPage--;
                return true;
            };

            return {
                root: book,
                open,
                close,
                nextPage,
                prevPage,
                get state() {
                    return {
                        isOpen,
                        currentPage,
                        pageCount
                    };
                }
            };
        }

        // Expose une fonction globale pour Blazor
        window.Book3D = {
            init: function (canvasId, opts) {
                try {
                    const canvas = document.getElementById(canvasId);
                    if (!canvas) {
                        throw new Error(`Canvas avec l'ID '${canvasId}' introuvable`);
                    }

                    const engine = new BABYLON.Engine(canvas, true, {
                        preserveDrawingBuffer: true,
                        stencil: true
                    });

                    const scene = new BABYLON.Scene(engine);
                    scene.clearColor = new BABYLON.Color4(0, 0, 0, 0); // fond transparent

                    const book = createBook(scene, opts || {});

                    engine.runRenderLoop(() => {
                        try {
                            scene.render();
                        } catch (error) {
                            console.error("Erreur lors du rendu:", error);
                        }
                    });

                    // Gestion du redimensionnement
                    const resizeHandler = () => {
                        try {
                            engine.resize();
                        } catch (error) {
                            console.error("Erreur lors du redimensionnement:", error);
                        }
                    };

                    window.addEventListener("resize", resizeHandler);

                    // Retourner une petite API utilisable depuis Blazor
                    return {
                        open: () => {
                            try {
                                return book.open();
                            } catch (error) {
                                console.error("Erreur lors de l'ouverture:", error);
                                throw error;
                            }
                        },
                        close: () => {
                            try {
                                return book.close();
                            } catch (error) {
                                console.error("Erreur lors de la fermeture:", error);
                                throw error;
                            }
                        },
                        nextPage: () => {
                            try {
                                return book.nextPage();
                            } catch (error) {
                                console.error("Erreur page suivante:", error);
                                throw error;
                            }
                        },
                        prevPage: () => {
                            try {
                                return book.prevPage();
                            } catch (error) {
                                console.error("Erreur page précédente:", error);
                                throw error;
                            }
                        },
                        getState: () => {
                            try {
                                return book.state;
                            } catch (error) {
                                console.error("Erreur état:", error);
                                throw error;
                            }
                        },
                        dispose: () => {
                            try {
                                window.removeEventListener("resize", resizeHandler);
                                engine.dispose();
                            } catch (error) {
                                console.error("Erreur dispose:", error);
                            }
                        }
                    };
                } catch (error) {
                    console.error("Erreur d'initialisation Book3D:", error);
                    throw error;
                }
            }
        };

        // Vérifier que Babylon.js est disponible
        if (typeof BABYLON === 'undefined') {
            console.error("BABYLON.js n'est pas chargé. Assurez-vous de l'inclure avant book3d.js");
        }
    })();

    }
}
