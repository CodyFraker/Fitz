import * as fs from 'fs'
import * as path from 'path'
import { execSync } from 'child_process'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
const SWAGGER_URL = `${API_URL}/swagger/v1/swagger.json`
const NSWAG_CONFIG_PATH = path.join(__dirname, '../nswag.json')
const OUTPUT_DIR = path.join(__dirname, '../src/lib/api/generated')
const OUTPUT_FILE = path.join(OUTPUT_DIR, 'api-client.ts')

async function generateApiClient() {
  try {
    console.log(`Fetching OpenAPI spec from ${SWAGGER_URL}...`)
    
    const response = await fetch(SWAGGER_URL)
    if (!response.ok) {
      throw new Error(`Failed to fetch OpenAPI spec: ${response.statusText}`)
    }

    const spec = await response.json()
    
    if (!fs.existsSync(OUTPUT_DIR)) {
      fs.mkdirSync(OUTPUT_DIR, { recursive: true })
    }

    const tempSpecPath = path.join(OUTPUT_DIR, 'openapi-spec.json')
    fs.writeFileSync(tempSpecPath, JSON.stringify(spec, null, 2))
    console.log(`Saved OpenAPI spec to ${tempSpecPath}`)

    const nswagConfig = JSON.parse(fs.readFileSync(NSWAG_CONFIG_PATH, 'utf-8'))
    nswagConfig.documentGenerator.fromDocument.json = tempSpecPath
    nswagConfig.documentGenerator.fromDocument.url = null
    nswagConfig.codeGenerators.openApiToTypeScriptClient.output = 'src/lib/api/generated/api-client.ts'

    const tempConfigPath = path.join(OUTPUT_DIR, 'nswag-temp.json')
    fs.writeFileSync(tempConfigPath, JSON.stringify(nswagConfig, null, 2))

    console.log('Generating TypeScript client with NSwag...')
    
    try {
      execSync(`npx nswag run ${tempConfigPath}`, {
        stdio: 'inherit',
        cwd: path.join(__dirname, '..'),
      })
    } catch (error: any) {
      if (error.status === 127 || error.message?.includes('nswag')) {
        console.log('NSwag not found via npx, trying dotnet tool...')
        try {
          execSync(`dotnet tool run nswag -- run ${tempConfigPath}`, {
            stdio: 'inherit',
            cwd: path.join(__dirname, '..'),
          })
        } catch (dotnetError) {
          throw new Error(
            'NSwag not found. Please install it either via:\n' +
            '  npm install -D nswag\n' +
            '  or\n' +
            '  dotnet tool install -g NSwag.ConsoleCore'
          )
        }
      } else {
        throw error
      }
    }

    fs.unlinkSync(tempConfigPath)
    fs.unlinkSync(tempSpecPath)

    console.log(`API client generated successfully at ${OUTPUT_FILE}`)
  } catch (error) {
    console.error('Error generating API client:', error)
    process.exit(1)
  }
}

generateApiClient()
