import { generate } from 'openapi-typescript-codegen'
import * as fs from 'fs'
import * as path from 'path'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
const SWAGGER_URL = `${API_URL}/swagger/v1/swagger.json`
const OUTPUT_DIR = path.join(__dirname, '../src/lib/api/generated')

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

    console.log('Generating TypeScript client...')
    await generate({
      input: spec,
      output: OUTPUT_DIR,
      httpClient: 'axios',
      useOptions: true,
      useUnionTypes: true,
      exportCore: true,
      exportServices: true,
      exportModels: true,
      exportSchemas: false,
    })

    console.log(`API client generated successfully in ${OUTPUT_DIR}`)
  } catch (error) {
    console.error('Error generating API client:', error)
    process.exit(1)
  }
}

generateApiClient()
