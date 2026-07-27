allprojects {
    repositories {
        google()
        mavenCentral()
    }
}

val newBuildDir: Directory =
    rootProject.layout.buildDirectory
        .dir("../../build")
        .get()
rootProject.layout.buildDirectory.value(newBuildDir)

subprojects {
    val newSubprojectBuildDir: Directory = newBuildDir.dir(project.name)
    project.layout.buildDirectory.value(newSubprojectBuildDir)
    // package:jni 1.x očekuje Kotlin ekstenziju, dok Flutterov AGP 9
    // compatibility mode isključuje ugrađeni Kotlin.
    if (name == "jni") {
        pluginManager.apply("org.jetbrains.kotlin.android")
    }
    // Stripeov opcionalni issuing modul navodi povučeni TapAndPay artefakt.
    // OpenAmp koristi PaymentSheet, ne issuing push provisioning.
    configurations.configureEach {
        exclude(
            group = "com.google.android.gms",
            module = "play-services-tapandpay",
        )
    }
}
subprojects {
    project.evaluationDependsOn(":app")
}

tasks.register<Delete>("clean") {
    delete(rootProject.layout.buildDirectory)
}
