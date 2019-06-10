pipeline {
  agent { node { label 'centos7-mono4' } }
  stages {
    stage('Init') {
      steps {
        sh 'sudo yum -y install rpm-build redhat-rpm-config rpmdevtools yum-utils'
        sh 'mkdir -p $WORKSPACE/build/{BUILD,RPMS,SOURCES,SPECS,SRPMS}'
        sh 'cp opensearch-client.spec $WORKSPACE/build/SPECS/opensearch-client.spec'
        sh 'spectool -g -R --directory $WORKSPACE/build/SOURCES $WORKSPACE/build/SPECS/opensearch-client.spec'
        script {
          def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
          if (env.BRANCH_NAME == 'master') 
            env.release = env.BUILD_NUMBER
          else
            env.release = "SNAPSHOT" + sdf
        }
      }
    }
    stage('Build') {
      steps {
        echo "Build .NET application"
        sh "msbuild /t:build /p:Configuration=DEBUG /Restore:true"
        sh 'cp -r Terradue.OpenSearch.Client/bin $WORKSPACE/build/SOURCES/'
        sh 'cp src/main/scripts/opensearch-client $WORKSPACE/build/SOURCES/'
        sh 'cp -r packages $WORKSPACE/build/SOURCES/'
      }
    }
    stage('Package') {
      steps {
        echo "Build package dependencies"
        sh "sudo yum-builddep -y $WORKSPACE/build/SPECS/opensearch-client.spec"
        echo "Build package"
        sh "sudo rpmbuild --define \"_topdir $WORKSPACE/build\" -ba --define '_branch ${env.BRANCH_NAME}' --define '_release ${env.release}' $WORKSPACE/build/SPECS/opensearch-client.spec"
        sh "rpm -qpl $WORKSPACE/build/RPMS/*/*.rpm"
      }
    }
    stage('Publish') {
      steps {
        echo 'Deploying'
        script {
            // Obtain an Artifactory server instance, defined in Jenkins --> Manage:
            def server = Artifactory.server "repository.terradue.com"

            // Read the upload specs:
            def uploadSpec = readFile 'artifactdeploy.json'

            // Upload files to Artifactory:
            def buildInfo = server.upload spec: uploadSpec

            // Publish the merged build-info to Artifactory
            server.publishBuildInfo buildInfo
        }
      }       
    }
  }
}